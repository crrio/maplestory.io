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

        public static ItemNameInfo Parse(WZObject c)
            => new ItemNameInfo() {
                Id = int.Parse(c.Name),
                Name = c.HasChild("name") ? c["name"].ValueOrDefault<string>(null) : null, Desc = c.HasChild("desc") ? c["desc"].ValueOrDefault<string>(null) : null
            };

        public static IEnumerable<ItemNameInfo> GetNames(WZFile stringFile)
        {
            return stringFile
                .ResolvePath("/Eqp.img/Eqp")
                .SelectMany(c => c)
                .Select(ItemNameInfo.Parse)
            //Etc
            .Concat(
            stringFile.ResolvePath("Etc.img/Etc")
                .Select(ItemNameInfo.Parse)
            )
            //Cash
            .Concat(
            stringFile.ResolvePath("Cash.img")
                .Select(ItemNameInfo.Parse)
            )
            //Ins
            .Concat(
            stringFile.ResolvePath("Ins.img")
                .Select(ItemNameInfo.Parse)
            )
            //Consume
            .Concat(
            stringFile.ResolvePath("Consume.img")
                .Select(ItemNameInfo.Parse)
            )
            //Pet
            .Concat(
            stringFile.ResolvePath("Pet.img")
                .Select(ItemNameInfo.Parse)
            );
        }
    }
}
