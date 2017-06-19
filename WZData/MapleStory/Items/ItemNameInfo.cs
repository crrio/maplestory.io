using reWZ;
using reWZ.WZProperties;
using System.Collections.Generic;
using ImageSharp;
using System.Linq;
using System;

namespace WZData.MapleStory.Items
{
    public class ItemNameInfo : ItemName
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

        public ItemInfo Info;
        public string[] RequiredJobs;
        public int? RequiredLevel;
        public bool IsCash;

        public int? RequiredGender
        {
            get {
                if (!TypeInfo.OverallCategory.Equals("equip", StringComparison.CurrentCultureIgnoreCase)) return null;

                if (Id / 1000000 != 1 && Id / 10000 != 254 || Id / 10000 == 119 || Id / 10000 == 168)
                    return 2;

                if (Id < 100000)
                {
                    // This definitely applies to hair. I *think* it applies to face aswell.
                    switch(Id % 100000 / 1000 % 10)
                    {
                        case 3:
                        case 5:
                        case 6:
                            return 0;
                        case 4:
                        case 7:
                        case 8:
                            return 1;
                        case 9:
                        default:
                            return 2;
                    }
                } else
                {
                    return Id / 1000 % 10;
                }
            }
        }
        
        public static ItemNameInfo Parse(WZObject stringItem, Func<int, MapleItem> getMetaInfo = null)
        {
            int itemId = int.Parse(stringItem.Name);
            MapleItem item = getMetaInfo != null ? getMetaInfo(itemId) : null;
            return new ItemNameInfo()
            {
                Id = itemId,
                Name = stringItem.HasChild("name") ? stringItem["name"].ValueOrDefault<string>(null) : null,
                Desc = stringItem.HasChild("desc") ? stringItem["desc"].ValueOrDefault<string>(null) : null,
                Info = item?.MetaInfo,
                RequiredJobs = item != null ? JobNameLookup.Where(b => (b.Key & item.MetaInfo.Equip.reqJob) == b.Key).Select(b => b.Value).ToArray() : null,
                RequiredLevel = item?.MetaInfo?.Equip?.reqLevel ?? 0,
                IsCash = item?.MetaInfo?.Cash?.cash ?? false
            };
        }

        public static IEnumerable<ItemNameInfo> GetNames(WZFile stringFile, Func<int, MapleItem> getMetaInfo = null)
        {
            // TODO: getMetaInfo for all items
            return stringFile
                .ResolvePath("/Eqp.img/Eqp")
                .SelectMany(c => c)
                .Select(c => ItemNameInfo.Parse(c, getMetaInfo))
            //Etc
            .Concat(
            stringFile.ResolvePath("Etc.img/Etc")
                .Select(c => ItemNameInfo.Parse(c, null))
            )
            //Cash
            .Concat(
            stringFile.ResolvePath("Cash.img")
                .Select(c => ItemNameInfo.Parse(c, null))
            )
            //Ins
            .Concat(
            stringFile.ResolvePath("Ins.img")
                .Select(c => ItemNameInfo.Parse(c, null))
            )
            //Consume
            .Concat(
            stringFile.ResolvePath("Consume.img")
                .Select(c => ItemNameInfo.Parse(c, null))
            )
            //Pet
            .Concat(
            stringFile.ResolvePath("Pet.img")
                .Select(c => ItemNameInfo.Parse(c, null))
            ).AsParallel();
        }
    }
}
