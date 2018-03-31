using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using maplestory.io.Data;
using PKG1;
using System.Collections.Concurrent;
using maplestory.io.Entities;
using maplestory.io.Models;
using maplestory.io.Entities.Models;
using maplestory.io.Services.Interfaces.MapleStory;
using System.Threading;

namespace maplestory.io.Services.Implementations.MapleStory
{
    public class WZFactory : IWZFactory
    {
        public static ILogger Logger;
        static ConcurrentDictionary<string, EventWaitHandle> wzLoading = new ConcurrentDictionary<string, EventWaitHandle>();

        public static void AddWz(string basePath, Region region, string version) {
            if (!cache.ContainsKey(region))
                cache.TryAdd(region, new ConcurrentDictionary<string, MSPackageCollection>());
            ConcurrentDictionary<string, MSPackageCollection> versions = cache[region];
            versions.TryAdd(version, new MSPackageCollection(basePath));
        }

        private readonly ApplicationDbContext _ctx;

        public WZFactory(ApplicationDbContext ctx) => _ctx = ctx;
        static ConcurrentDictionary<Region, ConcurrentDictionary<string, MSPackageCollection>> cache = new ConcurrentDictionary<Region, ConcurrentDictionary<string, MSPackageCollection>>();

        public MSPackageCollection GetWZ(Region region, string version)
        {
            int regionNum = (int)region;
            version = version.TrimStart('0');

            EventWaitHandle wait = new EventWaitHandle(false, EventResetMode.ManualReset);
            string versionHash = $"{region.ToString()}-{version}";

            if (!cache.ContainsKey(region)) cache.TryAdd(region, new ConcurrentDictionary<string, MSPackageCollection>());
            else if (cache[region].ContainsKey(version))
                return cache[region][version];

            if (!wzLoading.TryAdd(versionHash, wait))
            {
                Logger.LogInformation($"Waiting for other thread to finish loading {region} - {version}");
                wzLoading[versionHash].WaitOne();
                Logger.LogInformation($"Finished waiting for {region} - {version}");
                if (cache.ContainsKey(region) && cache[region].ContainsKey(version))
                    return GetWZ(region, version);
                else throw new KeyNotFoundException("That version or region could not be found");
            }

            MapleVersion ver = null;

            if (version == "latest") ver = _ctx.MapleVersions.LastOrDefault(c => c.Region == regionNum);
            else ver = _ctx.MapleVersions.FirstOrDefault(c => c.Region == regionNum && c.MapleVersionId == version);

            if (ver == null)
            {
                wait.Set();
                throw new KeyNotFoundException("That version or region could not be found");
            }
            MSPackageCollection collection = new MSPackageCollection(ver, null, region);
            Logger.LogInformation($"Finished loading {region} - {version}");
            wait.Set();
            if (cache[region].TryAdd(version, collection))
            {
                if (ver.Id > cache[region]["latest"].MapleVersion.Id) // Update the latest pointer if this is newer than the old latest
                    cache[region]["latest"] = collection;
                return collection;
            }
            else return cache[region][version];
        }

        internal static void LoadAllWZ()
        {
            MapleVersion[] versions;
            using (ApplicationDbContext dbCtx = new ApplicationDbContext())
                versions = dbCtx.MapleVersions.ToArray();

            MapleVersion highest = versions.Select(c =>
            {
                if (int.TryParse(c.MapleVersionId, out int versionId))
                    return new Tuple<int, MapleVersion>(versionId, c);
                return null;
            }).OrderBy(c => c.Item1).Where(c => c != null).Last().Item2;

            Parallel.ForEach(versions, ver =>
            {
                Region region = (Region)ver.Region;
                string version = ver.MapleVersionId;

                if (!cache.ContainsKey(region)) cache.TryAdd(region, new ConcurrentDictionary<string, MSPackageCollection>());
                else if (cache[region].ContainsKey(version))
                    return;
                try
                {
                    MSPackageCollection collection = new MSPackageCollection(ver, null, region);
                    cache[region].TryAdd(version, collection);
                    if (ver == highest) cache[region].TryAdd("latest", collection);
                    Logger.LogInformation($"Finished loading {region} - {version}");
                }
                catch (Exception)
                {
                    Logger.LogWarning($"Couldn't load {region} - {version}");
                }
            });
        }

        public Dictionary<string, string[]> GetAvailableRegionsAndVersions() => cache.ToDictionary(c => c.Key.ToString(), c => c.Value.Keys.ToArray());

        public static MSPackageCollection GetWZFromCache(Region region, string version)
        {
            if (cache.ContainsKey(region) && cache[region].ContainsKey(version)) return cache[region][version];
            else return null;
        }
    }
}
