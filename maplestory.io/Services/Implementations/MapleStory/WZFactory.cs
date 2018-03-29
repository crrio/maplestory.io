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

namespace maplestory.io.Services.Implementations.MapleStory
{
    public class WZFactory : IWZFactory
    {
        private static ILogger _logger;

        public static void Load(ILogger<WZFactory> logger)
        {
            _logger = logger;
        }
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

            if (!cache.ContainsKey(region)) cache.TryAdd(region, new ConcurrentDictionary<string, MSPackageCollection>());
            else if (cache[region].ContainsKey(version))
                return cache[region][version];

            MapleVersion ver = null;

            if (version == "latest") ver = _ctx.MapleVersions.LastOrDefault(c => c.Region == regionNum);
            else ver = _ctx.MapleVersions.FirstOrDefault(c => c.Region == regionNum && c.MapleVersionId == version);

            if (ver == null) throw new KeyNotFoundException("That version or region could not be found");
            MSPackageCollection collection = new MSPackageCollection(_ctx, ver, null, region);
            if (cache[region].TryAdd(version, collection)) return collection;
            else return cache[region][version];
        }

        public Dictionary<string, string[]> GetAvailableRegionsAndVersions() => cache.ToDictionary(c => c.Key.ToString(), c => c.Value.Keys.ToArray());

        public static MSPackageCollection GetWZFromCache(Region region, string version)
        {
            if (cache.ContainsKey(region) && cache[region].ContainsKey(version)) return cache[region][version];
            else return null;
        }
    }
}
