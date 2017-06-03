using reWZ;
using reWZ.WZProperties;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace WZData.MapleStory.Items
{
    public class ItemName
    {
        public string Name, Desc;
        public int Id;
        public ItemInfo Info;

        public static ItemName Parse(WZObject c)
            => new ItemName() { Id = int.Parse(c.Name), Name = c.HasChild("name") ? c["name"].ValueOrDefault<string>(null) : null, Desc = c.HasChild("desc") ? c["desc"].ValueOrDefault<string>(null) : null };

        public static IEnumerable<ItemName> GetNames(WZFile stringFile)
        {
            return stringFile
                .ResolvePath("/Eqp.img/Eqp")
                .SelectMany(c => c)
                .Select(ItemName.Parse)
            //Etc
            .Concat(
            stringFile.ResolvePath("Etc.img/Etc")
                .Select(ItemName.Parse)
            )
            //Cash
            .Concat(
            stringFile.ResolvePath("Cash.img")
                .Select(ItemName.Parse)
            )
            //Ins
            .Concat(
            stringFile.ResolvePath("Ins.img")
                .Select(ItemName.Parse)
            )
            //Consume
            .Concat(
            stringFile.ResolvePath("Consume.img")
                .Select(ItemName.Parse)
            )
            //Pet
            .Concat(
            stringFile.ResolvePath("Pet.img")
                .Select(ItemName.Parse)
            );
        }
    }
}
