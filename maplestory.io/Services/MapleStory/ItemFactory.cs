using Microsoft.Extensions.Logging;
using MoreLinq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;
using WZData;
using WZData.MapleStory.Items;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using WZData.MapleStory.Images;
using PKG1;
using ImageSharp;

namespace maplestory.io.Services.MapleStory
{
    public class ItemFactory : NeedWZ<IItemFactory>, IItemFactory
    {
        public static Dictionary<int, string> JobNameLookup = new Dictionary<int, string>()
        {
            { 0, "Beginner" },
            { 1, "Warrior" },
            { 2, "Magician"},
            { 4, "Bowman" },
            { 8, "Thief" },
            { 16, "Pirate" }
        };
        private static Dictionary<Region, Dictionary<int, Tuple<string[], byte?, bool>>> RequiredJobs = new Dictionary<Region, Dictionary<int, Tuple<string[], byte?, bool>>>();

        public static void CacheEquipMeta(IWZFactory factory, ILogger logging) {
            Region[] regions = (Region[])Enum.GetValues(typeof(Region));
            foreach (Region region in regions) {
                PackageCollection wz = factory.GetWZ(region, "latest");

                logging.LogInformation($"Caching {region} - {wz}");
                if (wz == null) return;

                ConcurrentDictionary<int, Tuple<string[], byte?, bool>> regionData = new ConcurrentDictionary<int, Tuple<string[], byte?, bool>>();

                while(!Parallel.ForEach(
                    wz.Resolve("Character").Children.Values
                        .SelectMany(c => c.Children),
                    c => {
                        if (!int.TryParse(c.Key, out int itemId)) return;
                        int reqJob = c.Value.ResolveFor<int>("info/reqJob") ?? 0;
                        regionData.TryAdd(
                            itemId,
                            new Tuple<string[], byte?, bool>(
                                JobNameLookup.Where(b => (b.Key & reqJob) == b.Key && (b.Key != 0 || reqJob == 0)).Select(b => b.Value).ToArray(),
                                c.Value.ResolveFor<byte>("info/reqLevel"),
                                c.Value.ResolveFor<bool>("info/cash") ?? false
                            )
                        );
                    }
                ).IsCompleted) Thread.Sleep(1);

                RequiredJobs.Add(region, new Dictionary<int, Tuple<string[], byte?, bool>>(regionData));

                logging.LogInformation($"Found {RequiredJobs[region].Count} items for {region}, latest");
            }
        }

        public ItemFactory(IWZFactory factory) : base(factory) { }
        public ItemFactory(IWZFactory _factory, Region region, string version) : base(_factory, region, version) { }

        public IEnumerable<string> GetItemCategories() => ItemType.overall.Keys;

        public IEnumerable<ItemNameInfo> GetItems() {
            WZProperty stringWz = wz.Resolve("String");
            IEnumerable<WZProperty> eqp = stringWz.Resolve("Eqp/Eqp").Children.Values.SelectMany(c => c.Children.Values);
            IEnumerable<WZProperty> etc = stringWz.Resolve("Etc/Etc").Children.Values;
            IEnumerable<WZProperty> ins = stringWz.Resolve("Ins").Children.Values;
            IEnumerable<WZProperty> cash = stringWz.Resolve("Cash").Children.Values;
            IEnumerable<WZProperty> consume = stringWz.Resolve("Consume").Children.Values;
            IEnumerable<WZProperty> pet = stringWz.Resolve("Pet").Children.Values;

            IEnumerable<WZProperty> allItems = eqp.Concat(etc).Concat(ins).Concat(cash).Concat(consume).Concat(pet);

            return allItems.Select(c => {
                ItemNameInfo name = ItemNameInfo.Parse(c);
                if (RequiredJobs.ContainsKey(region) && RequiredJobs[region].ContainsKey(name.Id)) {
                    name.RequiredJobs = RequiredJobs[region][name.Id].Item1;
                    name.RequiredLevel = RequiredJobs[region][name.Id].Item2;
                    name.IsCash = RequiredJobs[region][name.Id].Item3;
                }
                return name;
            });
        }

        public MapleItem search(int id) {
            WZProperty stringWz = wz.Resolve("String");

            string idString = id.ToString();

            WZProperty item = stringWz.Resolve("Eqp/Eqp").Children.Values.FirstOrDefault(c => c.Children.ContainsKey(idString))?.Resolve(idString);
            if (item != null) return Equip.Parse(item);

            item = stringWz.Resolve($"Etc/Etc/{idString}");
            if (item != null) return Etc.Parse(item);

            item = stringWz.Resolve($"Ins/{idString}");
            if (item != null) return Install.Parse(item);

            item = stringWz.Resolve("Cash/{idString}");
            if (item != null) return Cash.Parse(item);

            item = stringWz.Resolve($"Consume/{idString}");
            if (item != null) return Consume.Parse(item);

            item = stringWz.Resolve($"Pet/{idString}");
            if (item != null) return Pet.Parse(item);

            return null;
        }

        public override IItemFactory GetWithWZ(Region region, string version)
            => new ItemFactory(_factory, region, version);

        WZProperty GetItemNode(int id) {
            WZProperty stringWz = wz.Resolve("String");

            string idString = id.ToString("D8");
            WZProperty item = wz.Resolve("Character").Children.Values.SelectMany(c => c.Children.Values).Where(c => c.Type == PropertyType.Image).FirstOrDefault(c => c.Name == idString);
            if (item != null) return item;

            item = wz.Resolve($"Item/Etc/{idString.Substring(0, 4)}/{idString}");
            if (item != null) return item;

            item = wz.Resolve($"Item/Install/{idString.Substring(0, 4)}/{idString}");
            if (item != null) return item;

            item = wz.Resolve($"Item/Cash/{idString.Substring(0, 4)}/{idString}");
            if (item != null) return item;

            item = wz.Resolve($"Item/Consume/{idString.Substring(0, 4)}/{idString}");
            if (item != null) return item;

            item = wz.Resolve($"Item/Special/{idString.Substring(0, 4)}/{idString}");
            if (item != null) return item;

            item = wz.Resolve($"Item/Pet/{idString}");
            if (item != null) return item;

            return null;
        }

        public Image<Rgba32> GetIcon(int itemId)
        {
            WZProperty itemNode = GetItemNode(itemId);
            Image<Rgba32> icon = itemNode.ResolveForOrNull<Image<Rgba32>>("info/icon");
            if (icon == null) {
                WZProperty action = itemNode.Children.Values.First(c => c.Name != "info");
                return EquipFrameBook.Parse(action).frames?.FirstOrDefault()?.Effects?.Values.FirstOrDefault()?.Image;
            }
            return icon;
        }

        public Image<Rgba32> GetIconRaw(int itemId)
        {
            WZProperty itemNode = GetItemNode(itemId);
            return itemNode.ResolveForOrNull<Image<Rgba32>>("info/iconRaw");
        }

        public bool DoesItemExist(int itemId)
            => GetItemNode(itemId) != null;
    }
}
