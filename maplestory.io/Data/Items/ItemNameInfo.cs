using System.Collections.Generic;
using SixLabors.ImageSharp;
using System.Linq;
using System;
using PKG1;

namespace maplestory.io.Data.Items
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
                Id = int.Parse(c.NameWithoutExtension),
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
                IEnumerable<WZProperty> eqp = (stringFile.Resolve("Eqp/Eqp") ?? stringFile.Resolve("Item/Eqp")).Children.SelectMany(c => c.Children);
                IEnumerable<WZProperty> etc = (stringFile.Resolve("Etc/Etc") ?? stringFile.Resolve("Item/Etc")).Children;
                IEnumerable<WZProperty> ins = (stringFile.Resolve("Ins") ?? stringFile.Resolve("Item/Ins")).Children;
                IEnumerable<WZProperty> cash = (stringFile.Resolve("Cash") ?? stringFile.Resolve("Item/Cash")).Children;
                IEnumerable<WZProperty> consume = (stringFile.Resolve("Consume") ?? stringFile.Resolve("Item/Con")).Children;
                IEnumerable<WZProperty> pet = (stringFile.Resolve("Pet") ?? stringFile.Resolve("Item/Pet")).Children;

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
                itemNameLookup = (stringFile.Resolve("Eqp.img/Eqp") ?? stringFile.Resolve("Item/Eqp")).Children
                    .SelectMany(c => c.Children)
                //Etc
                .Concat((stringFile.Resolve("Etc.img/Etc") ?? stringFile.Resolve("Item.img/Etc")).Children)
                //Cash
                .Concat((stringFile.Resolve("Cash.img") ?? stringFile.Resolve("Item.img/Cash")).Children)
                //Ins
                .Concat((stringFile.Resolve("Ins.img") ?? stringFile.Resolve("Item.img/Ins")).Children)
                //Consume
                .Concat((stringFile.Resolve("Consume.img") ?? stringFile.Resolve("Item.img/Con")).Children)
                //Pet
                .Concat((stringFile.Resolve("Pet.img") ?? stringFile.Resolve("Item.img/Pet")).Children)
                .ToLookup(c => int.Parse(c.NameWithoutExtension), c => ItemNameInfo.Parse(c));
                stringFile.FileContainer.Collection.VersionCache.AddOrUpdate("itemNameLookup", itemNameLookup, (a, b) => b);
            }

            return itemNameLookup;
        }
    }
}
