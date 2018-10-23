using maplestory.io.Models;
using maplestory.io.Services.Interfaces.MapleStory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PKG1;
using SixLabors.ImageSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;

namespace maplestory.io.Services.Implementations.MapleStory
{
    public class WZAppSettingsFactory : IWZFactory
    {
        private WZOptions _config;

        public static ILogger Logger;
        static ConcurrentDictionary<string, EventWaitHandle> wzLoading = new ConcurrentDictionary<string, EventWaitHandle>();
        static ConcurrentDictionary<Region, ConcurrentDictionary<string, MSPackageCollection>> cache = new ConcurrentDictionary<Region, ConcurrentDictionary<string, MSPackageCollection>>();

        public WZAppSettingsFactory(IOptions<WZOptions> config) : this(config.Value) { }
        public WZAppSettingsFactory(WZOptions config) => this._config = config;

        public MSPackageCollection GetWZ(Region region, string version)
        {
            if (version == null)
            {
                version = "latest";
            }

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

            // If there's no version, default to latest.
            // TODO(acornwall): Verify that this is correct behavior.
            var maybeVersion = _config.versions.Where(c => c.region == region && c.version == version);
            WZVersion wzVersion =
                maybeVersion.FirstOr(_config.versions.First(c => c.region == region && c.version == "latest"));
            MSPackageCollection collection =
                new MSPackageCollection(
                    wzVersion.path,
                    ushort.TryParse(wzVersion.version, out ushort ver) ? (ushort?) ver : null,
                    wzVersion.region);

            Logger.LogInformation($"Finished loading {region} - {version}");
            if (cache[region].TryAdd(version, collection) && cache[region].ContainsKey("latest"))
            {
                wait.Set();
                return collection;
            }
            else return cache[region][version];
        }
    }
}
