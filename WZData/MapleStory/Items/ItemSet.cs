using System;
using System.Collections.Generic;
using System.Text;
using PKG1;
using WZData.ItemMetaInfo;
using System.IO;
using System.Linq;

namespace WZData.MapleStory.Items
{
    public class ItemSet
    {
        public IEnumerable<IEnumerable<ItemName>> RequiredItems;
        public string SetName;
        public int CompleteCount;
        // TODO: Flesh out set effect attributes
        // activeSkill + EquipInfo + Others?
        //public Dictionary<int, > Effects;

        public static ItemSet ParseItemInfo(WZProperty info)
        {
            if (!info.Children.ContainsKey("setItemID")) return null;

            return Parse(info.ResolveOutlink(Path.Combine("Etc", "SetItemInfo", (info.ResolveFor<int>("setItemID") ?? -1).ToString())));
        }

        public static ItemSet Parse(WZProperty set)
        {
            ItemSet result = new ItemSet();
            ILookup<int, ItemNameInfo> itemNameLookup = ItemNameInfo.GetNameLookup(set.ResolveOutlink("String"));

            result.SetName = set.ResolveForOrNull<string>("setItemName");
            result.CompleteCount = set.ResolveFor<int>("completeCount") ?? 1;
            result.RequiredItems = set.Resolve("ItemID").Children.Values.Select(c =>
            {
                if (c.Type == PropertyType.SubProperty)
                    return c.Children.Where(b => int.TryParse(b.Key, out int blah)).Select(b => b.Value.ResolveFor<int>() ?? -1);
                else
                    return new int[] { c.ResolveFor<int>() ?? -1 };
            }).Select(c => c.Select(b => itemNameLookup[b].First()));

            return result;
        }
    }
}
