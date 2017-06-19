using Microsoft.Extensions.Logging;
using MoreLinq;
using reWZ;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;
using WZData;
using WZData.MapleStory.Items;
using System.Linq;
using reWZ.WZProperties;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using WZData.MapleStory;

namespace maplestory.io.Services.MapleStory
{
    public class ItemFactory : IItemFactory
    {
        private static Dictionary<int, Func<MapleItem>> itemLookup;
        private static Dictionary<WZLanguage, List<ItemNameInfo>> itemDb;
        private static Dictionary<WZLanguage, List<ItemNameInfo>> alternativeLanguages;
        private static ILogger<ItemFactory> _logger;
        public static Thread backgroundCaching;
        private static Dictionary<int, Func<MapleItem>> equipLookup;
        private static ISkillFactory _skillFactory;

        public static void Load(IWZFactory factory, ILogger<ItemFactory> logger, bool preloadItems = true)
        {
            itemDb = new Dictionary<WZLanguage, List<ItemNameInfo>>();
            _logger = logger;

            Stopwatch watch = Stopwatch.StartNew();
            _logger?.LogInformation("Caching item lookup table");

            var tmpEquipLookup = Equip.GetLookup(factory.AsyncGetWZFile(WZ.Character), factory.AsyncGetWZFile(WZ.Effect), factory.GetWZFile(WZ.String), factory.GetWZFile(WZ.Character));
            equipLookup = tmpEquipLookup
                .DistinctBy((p) => p.Item1)
                .ToDictionary(a => a.Item1, a => a.Item2);

            itemLookup = tmpEquipLookup
            .Concat(Consume.GetLookup(factory.AsyncGetWZFile(WZ.Item), factory.GetWZFile(WZ.String).MainDirectory))
            .Concat(Etc.GetLookup(factory.AsyncGetWZFile(WZ.Item), factory.GetWZFile(WZ.String).MainDirectory))
            .Concat(Install.GetLookup(factory.AsyncGetWZFile(WZ.Item), factory.GetWZFile(WZ.String).MainDirectory))
            .Concat(Cash.GetLookup(factory.AsyncGetWZFile(WZ.Item), factory.GetWZFile(WZ.String).MainDirectory))
            .Concat(Pet.GetLookup(factory.GetWZFile(WZ.Item).MainDirectory, factory.GetWZFile(WZ.String).MainDirectory))
                .DistinctBy((p) => p.Item1)
                .ToDictionary(a => a.Item1, a => a.Item2);

            watch.Stop();
            _logger?.LogInformation($"Cached {itemLookup.Count} item lookups, took {watch.ElapsedMilliseconds}ms");
            _logger?.LogInformation($"Caching {itemLookup.Count} high level item information");
            watch.Restart();
            itemDb = new Dictionary<WZLanguage, List<ItemNameInfo>>();
            itemDb.Add(WZLanguage.English, ItemNameInfo.GetNames(
                factory.GetWZFile(WZ.String),
                null
            ).ToList());
            WZFile krStringWz = factory.GetWZFile(WZ.String, WZLanguage.Korean);
            if (krStringWz != null)
            {
                itemDb.Add(WZLanguage.Korean, ItemNameInfo.GetNames(
                    krStringWz,
                    null
                ).Where(c => equipLookup.ContainsKey(c.Id)).ToList());
            }
            WZFile jpStringWz = factory.GetWZFile(WZ.String, WZLanguage.Japanese);
            if (jpStringWz != null)
            {
                itemDb.Add(WZLanguage.Japanese, ItemNameInfo.GetNames(
                    jpStringWz,
                    null
                ).Where(c => equipLookup.ContainsKey(c.Id)).ToList());
            }
            _logger?.LogInformation($"Cached {itemLookup.Count} items, took {watch.ElapsedMilliseconds}ms");
            watch.Stop();
        }

        public static void cacheItems()
        {
            Stopwatch watch = Stopwatch.StartNew();
            _logger.LogWarning("Starting background caching of item meta info");
            int totalRemaining = itemLookup.Count(c => equipLookup.ContainsKey(c.Key));
            itemDb[WZLanguage.English].AsParallel()
                .Where(c => itemLookup.ContainsKey(c.Id) && equipLookup.ContainsKey(c.Id))
                .Select(c =>
                {
                    try
                    {
                        _logger.LogInformation($"Processing {c.Id}");
                         Equip item = (Equip)itemLookup[c.Id]();
                        Interlocked.Decrement(ref totalRemaining);
                        _logger.LogInformation($"Processed {c.Id}, Total remaining: {totalRemaining}");
                        return item;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error trying to cache item {0}", c.Id);
                    }
                    return null;
                }).ToArray();
            watch.Stop();
            Startup.Ready = true;
            _logger.LogInformation("Completed background caching of item meta info, took {0}", watch.ElapsedMilliseconds);
        }

        public IEnumerable<string> GetItemCategories() => ItemType.overall.Keys;

        public IEnumerable<ItemNameInfo> GetItems(WZLanguage language = WZLanguage.English) => itemDb[language];
        public MapleItem search(int id) => itemLookup.ContainsKey(id) ? itemLookup[id]() : null;
    }
}
