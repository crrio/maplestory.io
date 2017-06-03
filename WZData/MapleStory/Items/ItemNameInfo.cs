using reWZ;
using reWZ.WZProperties;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace WZData.MapleStory.Items
{
    public class ItemNameInfo : ItemName
    {
        public ItemInfo Info;

        public static ItemNameInfo Parse(WZObject c)
            => new ItemNameInfo() { Id = int.Parse(c.Name), Name = c.HasChild("name") ? c["name"].ValueOrDefault<string>(null) : null, Desc = c.HasChild("desc") ? c["desc"].ValueOrDefault<string>(null) : null };

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
