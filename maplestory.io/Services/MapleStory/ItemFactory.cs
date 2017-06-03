using Microsoft.Extensions.Logging;
using MoreLinq;
using reWZ;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using WZData;
using WZData.MapleStory.Items;
using System.Linq;
using reWZ.WZProperties;

namespace maplestory.io.Services.MapleStory
{
    public class ItemFactory : IItemFactory
    {
        private readonly ConcurrentDictionary<int, Func<bool, MapleItem>> itemLookup;
        private readonly List<ItemName> itemDb;
        private readonly ILogger<ItemFactory> _logger;

        public ItemFactory(IWZFactory factory, ILogger<ItemFactory> logger)
        {
            itemLookup = new ConcurrentDictionary<int, Func<bool, MapleItem>>();
            itemDb = new List<ItemName>();
            _logger = logger;

            Stopwatch watch = Stopwatch.StartNew();
            _logger.LogInformation("Caching item lookup table");

            Parallel.ForEach(Equip.GetLookup(factory.GetWZFile(WZ.Character).MainDirectory, factory.GetWZFile(WZ.String).MainDirectory).DistinctBy((p) => p.Item1), itemInstance => { while (itemLookup.TryAdd(itemInstance.Item1, itemInstance.Item2)); });
            Parallel.ForEach(Consume.GetLookup(factory.GetWZFile(WZ.Item).MainDirectory, factory.GetWZFile(WZ.String).MainDirectory).DistinctBy((p) => p.Item1), itemInstance => { while (itemLookup.TryAdd(itemInstance.Item1, (a) => itemInstance.Item2())); });
            Parallel.ForEach(Etc.GetLookup(factory.GetWZFile(WZ.Item).MainDirectory, factory.GetWZFile(WZ.String).MainDirectory).DistinctBy((p) => p.Item1), itemInstance => { while (itemLookup.TryAdd(itemInstance.Item1, (a) => itemInstance.Item2())); });
            Parallel.ForEach(Install.GetLookup(factory.GetWZFile(WZ.Item).MainDirectory, factory.GetWZFile(WZ.String).MainDirectory).DistinctBy((p) => p.Item1), itemInstance => { while (itemLookup.TryAdd(itemInstance.Item1, (a) => itemInstance.Item2())); });
            Parallel.ForEach(Cash.GetLookup(factory.GetWZFile(WZ.Item).MainDirectory, factory.GetWZFile(WZ.String).MainDirectory).DistinctBy((p) => p.Item1), itemInstance => { while (itemLookup.TryAdd(itemInstance.Item1, itemInstance.Item2)); });
            Parallel.ForEach(Pet.GetLookup(factory.GetWZFile(WZ.Item).MainDirectory, factory.GetWZFile(WZ.String).MainDirectory).DistinctBy((p) => p.Item1), itemInstance => { while (itemLookup.TryAdd(itemInstance.Item1, itemInstance.Item2)); });
            watch.Stop();
            _logger.LogInformation($"Cached {itemLookup.Count} item lookups, took {watch.ElapsedMilliseconds}ms");
            _logger.LogInformation($"Caching {itemLookup.Count} high level item information");
            watch.Restart();
            itemDb = ItemName.GetNames(factory.GetWZFile(WZ.String)).ToList();
            _logger.LogInformation($"Cached {itemLookup.Count} items, took {watch.ElapsedMilliseconds}ms");
        }

        public IEnumerable<ItemName> GetItems() => itemDb;
        public MapleItem search(int id) => itemLookup[id](true);
    }
}
