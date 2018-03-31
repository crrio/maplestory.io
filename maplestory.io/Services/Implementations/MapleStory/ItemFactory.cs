using maplestory.io.Services.Interfaces.MapleStory;
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
using maplestory.io.Data;
using maplestory.io.Data.Items;
using maplestory.io.Data.Images;
using maplestory.io.Data.Items;

namespace maplestory.io.Services.Implementations.MapleStory
{
    public class ItemFactory : NeedWZ, IItemFactory
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
            WZProperty stringWz = WZ.Resolve("String");

            string[] jobFilterNames = jobFilter == null ? null : JobNameLookup.Where(b => (b.Key & jobFilter) == b.Key && (b.Key != 0 || jobFilter == 0)).Select(b => b.Value).ToArray();

            // TODO: Refactor this
            IEnumerable<ItemNameInfo> results = ItemNameInfo.GetNames(stringWz).Select(name =>
            {
                if (WZ.EquipMeta.ContainsKey(name.Id))
                {
                    name.RequiredJobs = WZ.EquipMeta[name.Id].Item1;
                    name.RequiredLevel = WZ.EquipMeta[name.Id].Item2;
                    name.IsCash = WZ.EquipMeta[name.Id].Item3;
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

        public Task<MapleItem> SearchAsync(int id) => new Task<MapleItem>(() => Search(id));
        public MapleItem Search(int id) => Search(id, (i) => (WZ.ItemDrops?.ContainsKey(i) ?? false) ? WZ.ItemDrops[i].ToArray() : new int[0]);

        public Task<MapleItem> SearchAsync(int id, Func<int, int[]> getDroppedBy) => new Task<MapleItem>(() => Search(id, getDroppedBy));
        public MapleItem Search(int id, Func<int, int[]> getDroppedBy) {
            WZProperty stringWz = WZ.Resolve("String");

            string idString = id.ToString();
            MapleItem result = null;

            WZProperty item = (stringWz.Resolve("Eqp/Eqp") ?? stringWz.Resolve("Item/Eqp")).Children.FirstOrDefault(c => c.Children.Any(b => b.NameWithoutExtension.Equals(idString)))?.Resolve(idString);
            if (item != null) result = Equip.Parse(item);

            if (result == null)
            {
                item = (stringWz.Resolve("Etc/Etc") ?? stringWz.Resolve("Item/Etc"))?.Resolve(idString);
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

            MobFactory mobs = new MobFactory();
            mobs.CloneWZFrom(this);

            if (result != null && result.MetaInfo != null)
                result.MetaInfo.DroppedBy = getDroppedBy(id)?.Join(mobs.GetMobs().Where(c => c != null), c => c, c => c.Id, (a,b) => b)?.ToArray();

            return result;
        }

        public WZProperty GetItemNode(int id) {
            string idString = id.ToString("D8");
            // TODO: Refactor to use character grouping IDs
            WZProperty item = WZ.Resolve("Character").Children.SelectMany(c => c.Children).Where(c => c.Type == PropertyType.Image).FirstOrDefault(c => c.NameWithoutExtension == idString);
            if (item != null) return item;

            item = WZ.Resolve($"Item/Etc/{idString.Substring(0, 4)}/{idString}");
            if (item != null) return item;

            item = WZ.Resolve($"Item/Install/{idString.Substring(0, 4)}/{idString}");
            if (item != null) return item;

            item = WZ.Resolve($"Item/Cash/{idString.Substring(0, 4)}/{idString}");
            if (item != null) return item;

            item = WZ.Resolve($"Item/Consume/{idString.Substring(0, 4)}/{idString}");
            if (item != null) return item;

            item = WZ.Resolve($"Item/Special/{idString.Substring(0, 4)}/{idString}");
            if (item != null) return item;

            item = WZ.Resolve($"Item/Pet/{idString}");
            if (item != null) return item;

            return null;
        }

        public Image<Rgba32> GetIcon(int itemId)
        {
            WZProperty itemNode = GetItemNode(itemId);
            Image<Rgba32> icon = itemNode.ResolveForOrNull<Image<Rgba32>>("info/icon");
            if (icon == null) {
                WZProperty action = itemNode.Children.First(c => c.NameWithoutExtension != "info");
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
            WZProperty stringWz = WZ.Resolve("String");
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
