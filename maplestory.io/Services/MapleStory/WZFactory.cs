using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WZData;
using PKG1;

namespace maplestory.io.Services.MapleStory
{
    public class WZFactory : IWZFactory
    {
        public static Dictionary<Region, Dictionary<string, PackageCollection>> regions;
        private static ILogger _logger;

        public static void Load(ILogger<WZFactory> logger)
        {
            _logger = logger;
            regions = new Dictionary<Region, Dictionary<string, PackageCollection>>() {
                { Region.GMS, new Dictionary<string, PackageCollection>() },
                { Region.KMS, new Dictionary<string, PackageCollection>() },
                { Region.JMS, new Dictionary<string, PackageCollection>() }
            };
        }
        public static void AddWz(string basePath, Region region, string version) {
            if (!regions.ContainsKey(region))
                regions.Add(region, new Dictionary<string, PackageCollection>());
            Dictionary<string, PackageCollection> versions = regions[region];
            versions.Add(version, new PackageCollection(basePath));
        }
        public PackageCollection GetWZ(Region region, string version) {
            if (regions.ContainsKey(region))
            {
                Dictionary<string, PackageCollection> regionVersions = regions[region];

                if (regionVersions.ContainsKey(version))
                    return regionVersions[version];
            }

            return null;
        }

        public Dictionary<string, string[]> GetAvailableRegionsAndVersions() => regions.ToDictionary(c => c.Key.ToString(), c => c.Value.Keys.ToArray());
    }
}
