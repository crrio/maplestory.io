using System.Collections.Generic;
using ImageSharp;
using System.Linq;
using System;
using PKG1;

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

        public static ItemNameInfo Parse(WZProperty c)
            => new ItemNameInfo() {
                Id = int.Parse(c.Name),
                Name = c.ResolveForOrNull<string>("name"),
                Desc = c.ResolveForOrNull<string>("desc")
            };

        public static IEnumerable<ItemNameInfo> GetNames(WZProperty stringFile)
        {
            return stringFile
                .Resolve("Eqp.img/Eqp").Children
                .SelectMany(c => c.Value.Children)
                .Select(c => ItemNameInfo.Parse(c.Value))
            //Etc
            .Concat(
            stringFile.Resolve("Etc.img/Etc").Children
                .Select(c => ItemNameInfo.Parse(c.Value))
            )
            //Cash
            .Concat(
            stringFile.Resolve("Cash.img").Children
                .Select(c => ItemNameInfo.Parse(c.Value))
            )
            //Ins
            .Concat(
            stringFile.Resolve("Ins.img").Children
                .Select(c => ItemNameInfo.Parse(c.Value))
            )
            //Consume
            .Concat(
            stringFile.Resolve("Consume.img").Children
                .Select(c => ItemNameInfo.Parse(c.Value))
            )
            //Pet
            .Concat(
            stringFile.Resolve("Pet.img").Children
                .Select(c => ItemNameInfo.Parse(c.Value))
            );
        }
    }
}
