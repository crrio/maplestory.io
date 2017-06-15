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

namespace maplestory.io.Services.MapleStory
{
    public class ItemFactory : IItemFactory
    {
        private readonly Dictionary<int, Func<MapleItem>> itemLookup;
        private readonly List<ItemNameInfo> itemDb;
        private readonly ILogger<ItemFactory> _logger;
        public static Thread backgroundCaching;
        private readonly Dictionary<int, Func<MapleItem>> equipLookup;
        private readonly ISkillFactory _skillFactory;

        public ItemFactory(IWZFactory factory, ISkillFactory skillFactory, ILogger<ItemFactory> logger, IHostingEnvironment env)
        {
            itemDb = new List<ItemNameInfo>();
            _logger = logger;
            _skillFactory = skillFactory;

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
            itemDb = ItemNameInfo.GetNames(factory.GetWZFile(WZ.String)).ToList();
            _logger?.LogInformation($"Cached {itemLookup.Count} items, took {watch.ElapsedMilliseconds}ms");
            watch.Stop();
            if (!env.IsDevelopment())
            {
                backgroundCaching = new Thread(cacheItems);
                backgroundCaching.Start();
                Startup.Started = true;
            }
        }

        void cacheItems()
        {
            Stopwatch watch = Stopwatch.StartNew();
            _logger.LogWarning("Starting background caching of item meta info");
            int totalRemaining = itemLookup.Count(c => equipLookup.ContainsKey(c.Key));
            itemDb.AsParallel()
                .Where(c => itemLookup.ContainsKey(c.Id) && equipLookup.ContainsKey(c.Id))
                .Select(c =>
                {
                    try
                    {
                        _logger.LogInformation($"Processing {c.Id}");
                        Equip item = (Equip)itemLookup[c.Id]();
                        if (item != null)
                        {
                            c.Info = item.MetaInfo;
                            c.RequiredJob = _skillFactory.GetJob(item.MetaInfo.Equip.reqJob ?? 0);
                            c.RequiredLevel = item.MetaInfo.Equip.reqLevel ?? 0;
                        }
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

        public IEnumerable<ItemNameInfo> GetItems() => itemDb;
        public MapleItem search(int id) => itemLookup[id]();
    }
}
