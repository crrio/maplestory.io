using Microsoft.Extensions.Logging;
using MoreLinq;
using PKG1;
using SixLabors.ImageSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WZData;
using WZData.ItemMetaInfo;
using WZData.MapleStory.Images;
using WZData.MapleStory.Items;

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
        private static Dictionary<Region, Dictionary<decimal, ILookup<int, int>>> DroppedLookups = new Dictionary<Region, Dictionary<decimal, ILookup<int, int>>>();

        public static void CacheEquipMeta(IWZFactory factory, ILogger logging) {
            Region[] regions = (Region[])Enum.GetValues(typeof(Region));
            foreach (Region region in regions) {
                PackageCollection wz = factory.GetWZ(region, "latest");

                logging.LogInformation($"Caching {region} - {wz}");
                if (wz == null) continue;

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

        public Dictionary<string, Dictionary<string, Tuple<string, int, int>[]>> GetItemCategories() => ItemType.overall;

        public IEnumerable<ItemNameInfo> GetItems(
            uint startPosition = 0, 
            uint? count = null, 
            string overallCategoryFilter = null, 
            string categoryFilter = null, 
            string subCategoryFilter = null, 
            int? jobFilter = null, 
            bool? cashFilter = null, 
            int? minLevelFilter = null,
            int? maxLevelFilter = null, 
            int? genderFilter = null,
            string searchFor = null
        ) {
            WZProperty stringWz = wz.Resolve("String");

            string[] jobFilterNames = jobFilter == null ? null : JobNameLookup.Where(b => (b.Key & jobFilter) == b.Key && (b.Key != 0 || jobFilter == 0)).Select(b => b.Value).ToArray();

            IEnumerable<ItemNameInfo> results = ItemNameInfo.GetNames(stringWz).Select(name =>
            {
                if (RequiredJobs.ContainsKey(region) && RequiredJobs[region].ContainsKey(name.Id))
                {
                    name.RequiredJobs = RequiredJobs[region][name.Id].Item1;
                    name.RequiredLevel = RequiredJobs[region][name.Id].Item2;
                    name.IsCash = RequiredJobs[region][name.Id].Item3;
                }
                return name;
            })
            .Where(item =>
            {
                bool matchesFilter = true;
                matchesFilter &= overallCategoryFilter == null || item.TypeInfo.OverallCategory.Equals(overallCategoryFilter, StringComparison.CurrentCultureIgnoreCase);
                matchesFilter &= categoryFilter == null || item.TypeInfo.Category.Equals(categoryFilter, StringComparison.CurrentCultureIgnoreCase);
                matchesFilter &= subCategoryFilter == null || item.TypeInfo.SubCategory.Equals(subCategoryFilter, StringComparison.CurrentCultureIgnoreCase);
                matchesFilter &= jobFilter == null || (item.RequiredJobs?.SequenceEqual(jobFilterNames) ?? false);
                matchesFilter &= cashFilter == null || item.IsCash == cashFilter;
                matchesFilter &= minLevelFilter == null || minLevelFilter <= item.RequiredLevel;
                matchesFilter &= maxLevelFilter == null || maxLevelFilter >= item.RequiredLevel;
                matchesFilter &= genderFilter == null || item.RequiredGender == genderFilter;
                matchesFilter &= searchFor == null || (item.Name?.ToLower().Contains(searchFor.ToLower()) ?? false) || (item.Desc?.ToLower().Contains(searchFor.ToLower()) ?? false);

                return matchesFilter;
            })
            .Skip((int)startPosition);

            if (count != null && count.HasValue) return results.Take((int)count.Value);
            return results;
        }

        ILookup<int, int> GenerateDroppedByLookup(WZProperty prop) {
            return prop.ResolveOutlink("String/MonsterBook")?
                .Children.Values
                .SelectMany(c =>
                    c.Resolve("reward").Children.Values
                    .Select(b =>
                        new Tuple<int, int?>(int.Parse(c.Name), b.ResolveFor<int>())
                    ).Where(b => b.Item2 != null)
                )
                .ToLookup(c => c.Item2.Value, c => c.Item1);
        }

        public MapleItem search(int id) {
            if (DroppedLookups.ContainsKey(wz.WZRegion) && DroppedLookups[wz.WZRegion].ContainsKey(wz.BasePackage.VersionId))
                return search(id, (i) => DroppedLookups[wz.WZRegion][wz.BasePackage.VersionId]?[i]?.ToArray());
            else {
                if (!DroppedLookups.ContainsKey(wz.WZRegion))
                    DroppedLookups.Add(wz.WZRegion, new Dictionary<decimal, ILookup<int, int>>());
                if (!DroppedLookups[wz.WZRegion].ContainsKey(wz.BasePackage.VersionId))
                    DroppedLookups[wz.WZRegion].Add(wz.BasePackage.VersionId, GenerateDroppedByLookup(wz.BasePackage.MainDirectory));

                return search(id);
            }
        }

        public MapleItem search(int id, Func<int, int[]> getDroppedBy) {
            WZProperty stringWz = wz.Resolve("String");

            string idString = id.ToString();
            MapleItem result = null;

            WZProperty item = (stringWz.Resolve("Eqp/Eqp") ?? stringWz.Resolve("Item/Eqp")).Children.Values.FirstOrDefault(c => c.Children.ContainsKey(idString))?.Resolve(idString);
            if (item != null) result = Equip.Parse(item);

            if (result == null)
            {
                item = (stringWz.Resolve("Etc/Etc") ?? stringWz.Resolve("Item/Etc")).Resolve(idString);
                if (item != null) result = Etc.Parse(item);
            }

            if (result == null)
            {
                item = (stringWz.Resolve("Ins") ?? stringWz.Resolve("Item/Ins")).Resolve(idString);
                if (item != null) result = Install.Parse(item);
            }

            if (result == null)
            {
                item = (stringWz.Resolve("Cash") ?? stringWz.Resolve("Item/Cash")).Resolve(idString);
                if (item != null) result = Cash.Parse(item);
            }

            if (result == null)
            {
                item = (stringWz.Resolve("Consume") ?? stringWz.Resolve("Item/Con")).Resolve(idString);
                if (item != null) result = Consume.Parse(item);
            }

            if (result == null)
            {
                item = (stringWz.Resolve("Pet") ?? stringWz.Resolve("Item/Pet")).Resolve(idString);
                if (item != null) result = Pet.Parse(item);
            }

            MobFactory mobs = new MobFactory(_factory, region, version);

            if (result != null && result.MetaInfo != null)
                result.MetaInfo.DroppedBy = getDroppedBy(id)?.Join(mobs.GetMobs().Where(c => c != null), c => c, c => c.Id, (a,b) => b)?.ToArray();

            return result;
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

        public Tuple<ItemNameInfo, IconInfo, EquipInfo>[] BulkItemInfo(int[] itemIds)
        {
            WZProperty stringWz = wz.Resolve("String");
            ILookup<int,ItemNameInfo> nameLookup = ItemNameInfo.GetNameLookup(stringWz);
            return itemIds.Select(c =>
            {
                ItemNameInfo name = nameLookup[c].FirstOrDefault();
                if (name != null)
                    return new Tuple<int, ItemNameInfo>(c, name);
                return null;
            }).Where(c => c != null)
            .Select(c => {
                WZProperty info = GetItemNode(c.Item1).Resolve("info");
                return new Tuple<ItemNameInfo, IconInfo, EquipInfo>(c.Item2, IconInfo.Parse(info), EquipInfo.Parse(info));
            })
            .ToArray();
        }
    }
}
