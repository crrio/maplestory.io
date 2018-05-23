using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PKG1;

namespace maplestory.io.Data.Items
{
    public class Consume : MapleItem
    {
        public Consume(int id) : base(id) { }
        public static Consume Parse(WZProperty stringWz)
        {
            int id;

            if (!int.TryParse(stringWz.NameWithoutExtension, out id)) return null;

            Consume item = new Consume(id);

            WZProperty itemWz = stringWz.ResolveOutlink($"Item/Consume/{id.ToString("D8").Substring(0, 4)}.img/{id.ToString("D8")}");

            if (itemWz.Children.Any(c => c.NameWithoutExtension.Equals("info"))) item.MetaInfo = ItemInfo.Parse(itemWz);
            item.Description = ItemDescription.Parse(stringWz, id);

            return item;
        }
    }
}
