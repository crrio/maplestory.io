using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PKG1;

namespace WZData.MapleStory.Items
{
    public class Consume : MapleItem
    {
        public Consume(int id) : base(id) { }
        public static Consume Parse(WZProperty stringWz)
        {
            int id;

            if (!int.TryParse(stringWz.Name, out id)) return null;

            Consume item = new Consume(id);

            WZProperty itemWz = stringWz.ResolveOutlink($"Item/Consume/{id.ToString("D8").Substring(0, 4)}.img/{id.ToString("D8")}");

            if (itemWz.Children.ContainsKey("info")) item.MetaInfo = ItemInfo.Parse(itemWz);
            item.Description = ItemDescription.Parse(stringWz, id);

            return item;
        }
    }
}
