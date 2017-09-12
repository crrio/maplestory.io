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
                } else if (Id / 1000000 != 1 && (Id / 10000 != 254 || Id / 10000 == 119 || Id / 10000 == 168))
                    return 2;
                {
                    return Id / 1000 % 10;
                }
            }
        }

        public static ItemNameInfo Parse(WZProperty c)
            => new ItemNameInfo() {
                Id = int.Parse(c.Name),
                Name = c.ResolveForOrNull<string>("name"),
                Desc = string.Join("", c.ResolveForOrNull<string>("desc") ?? "", c.ResolveForOrNull<string>("autodesc") ?? "")
            };

        public static IEnumerable<ItemNameInfo> GetNames(WZProperty stringFile)
        {
            IEnumerable<ItemNameInfo> itemNames = null;
            if (stringFile.FileContainer.Collection.VersionCache.TryGetValue("itemNames", out object itemNamesCached))
                itemNames = (IEnumerable<ItemNameInfo>)itemNamesCached;
            else
            {
                IEnumerable<WZProperty> eqp = stringFile.Resolve("Eqp/Eqp").Children.Values.SelectMany(c => c.Children.Values);
                IEnumerable<WZProperty> etc = stringFile.Resolve("Etc/Etc").Children.Values;
                IEnumerable<WZProperty> ins = stringFile.Resolve("Ins").Children.Values;
                IEnumerable<WZProperty> cash = stringFile.Resolve("Cash").Children.Values;
                IEnumerable<WZProperty> consume = stringFile.Resolve("Consume").Children.Values;
                IEnumerable<WZProperty> pet = stringFile.Resolve("Pet").Children.Values;

                IEnumerable<WZProperty> allItems = eqp.Concat(etc).Concat(ins).Concat(cash).Concat(consume).Concat(pet);
                itemNames = allItems.Select(ItemNameInfo.Parse);

                stringFile.FileContainer.Collection.VersionCache.AddOrUpdate("itemNames", itemNames, (a, b) => b);
            }

            return itemNames;
        }

        public static ILookup<int, ItemNameInfo> GetNameLookup(WZProperty stringFile)
        {
            ILookup<int, ItemNameInfo> itemNameLookup = null;
            if (stringFile.FileContainer.Collection.VersionCache.TryGetValue("itemNameLookup", out object itemNameLookupCached))
                itemNameLookup = (ILookup<int, ItemNameInfo>)itemNameLookupCached;
            else
            {
                itemNameLookup = stringFile
                    .Resolve("Eqp.img/Eqp").Children
                    .SelectMany(c => c.Value.Children)
                //Etc
                .Concat(stringFile.Resolve("Etc.img/Etc").Children)
                //Cash
                .Concat(stringFile.Resolve("Cash.img").Children)
                //Ins
                .Concat(stringFile.Resolve("Ins.img").Children)
                //Consume
                .Concat(stringFile.Resolve("Consume.img").Children)
                //Pet
                .Concat(stringFile.Resolve("Pet.img").Children)
                .ToLookup(c => int.Parse(c.Key), c => ItemNameInfo.Parse(c.Value));
                stringFile.FileContainer.Collection.VersionCache.AddOrUpdate("itemNameLookup", itemNameLookup, (a, b) => b);
            }

            return itemNameLookup;
        }
    }
}
