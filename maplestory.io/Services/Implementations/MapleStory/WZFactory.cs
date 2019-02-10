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
        /// <summary>
        /// ASP Net Logger Placeholder
        /// </summary>
        public static ILogger Logger;

        /// <summary>
        /// Loading elements
        /// </summary>
        static ConcurrentDictionary<string, EventWaitHandle> wzLoading = new ConcurrentDictionary<string, EventWaitHandle>();

        /// <summary>
        /// Add single wizet stuff to cache.
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="region"></param>
        /// <param name="version"></param>
        public static void AddWz(string basePath, Region region, string version) {
            // Check to see if the cache contains the region.
            if (!cache.ContainsKey(region))
            {
                // Add the region to the cache with empty data.
                cache.TryAdd(region, new ConcurrentDictionary<string, MSPackageCollection>());
            }
            
            // Populate the local versions variable with all the versions in the region.
            ConcurrentDictionary<string, MSPackageCollection> versions = cache[region];
            versions.TryAdd(version, new MSPackageCollection(basePath));
        }

        /// <summary>
        /// Database context handler
        /// This handles the interation between the database and the application.
        /// </summary>
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext"></param>
        public WZFactory(ApplicationDbContext dbContext) => _dbContext = dbContext;

        /// <summary>
        /// Class Placeholder Dictionary for regions in the cache
        /// </summary>
        static ConcurrentDictionary<Region, ConcurrentDictionary<string, MSPackageCollection>> cache = new ConcurrentDictionary<Region, ConcurrentDictionary<string, MSPackageCollection>>();

        /// <summary>
        /// Get WZ File.
        /// </summary>
        /// <param name="region"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public MSPackageCollection GetWZ(Region region, string version)
        {
            // Make sure that we have a version.
            if (version == null) version = "latest";

            // Get the region number
            int regionNum = (int)region;

            // Trim the versions tring.
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

            // Placeholder variable.
            MapleVersion mapleVersion = null;

            if (version == "latest")
            {
                mapleVersion = _dbContext.MapleVersions.LastOrDefault(c => c.Region == regionNum);
            }
            else
            {
                mapleVersion = _dbContext.MapleVersions.FirstOrDefault(c => c.Region == regionNum && c.MapleVersionId == version);
            }

            if (mapleVersion == null)
            {
                wait.Set();
                throw new KeyNotFoundException("That version or region could not be found");
            }

            MSPackageCollection collection = new MSPackageCollection(mapleVersion, null, region);
            Logger.LogInformation($"Finished loading {region} - {version}");
            if (cache[region].TryAdd(version, collection) && cache[region].ContainsKey("latest"))
            {
                wait.Set();
                if (mapleVersion.Id > cache[region]["latest"].MapleVersion.Id)
                {
                    // Update the latest pointer if this is newer than the old latest
                    cache[region]["latest"] = collection;
                }
                
                // Return the new collection.
                return collection;
            }
            else
            {
                // Already exists, return the cached value.
                return cache[region][version];
            }
        }

        /// <summary>
        /// Load all wizet files.
        /// </summary>
        internal static void LoadAllWZ()
        {
            // Placeholder array
            MapleVersion[] versions;

            // Create a new database context and populate the versions.
            using (ApplicationDbContext localDbContext = new ApplicationDbContext())
            {
                versions = localDbContext.MapleVersions.ToArray();
            }
                
            // Find the newest version for the region.
            MapleVersion highest = versions.Select(c =>
            {
                if (int.TryParse(c.MapleVersionId, out int versionId))
                {
                    return new Tuple<int, MapleVersion>(versionId, c);
                } else
                {
                    return null;
                }
            }).OrderBy(c => c.Item1).Where(c => c != null).Last().Item2;

            // Iterate through each version
            Parallel.ForEach(versions, ver =>
            {
                Region region = (Region)ver.Region;
                string version = ver.MapleVersionId;

                // Check to see if the cache contains the region
                if (!cache.ContainsKey(region))
                {
                    // Add the region to the cache
                    cache.TryAdd(region, new ConcurrentDictionary<string, MSPackageCollection>());
                }
                // Check to see if the cached region contains the version.
                else if (cache[region].ContainsKey(version))
                {
                    // Return nothing and break out of the scope if it does.
                    return;
                }
                    
                // Try to load the version.
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

        /// <summary>
        /// Get wizet file from the cache.
        /// </summary>
        /// <param name="region"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static MSPackageCollection GetWZFromCache(Region region, string version)
        {
            // Check to see if the region and versione xists.
            if (cache.ContainsKey(region) && cache[region].ContainsKey(version))
            {
                // Return the cached version
                return cache[region][version];
            }
            else
            {
                // Return nothing.
                return null;
            }
        }
    }
}
