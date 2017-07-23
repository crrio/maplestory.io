using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PKG1;

namespace WZData.MapleStory.Items
{
    public class Etc : MapleItem
    {
        public Etc(int id) : base(id) { }
        public static Etc Parse(WZProperty stringWz)
        {
            int id;

            if (!int.TryParse(stringWz.Name, out id)) return null;

            Etc item = new Etc(id);

            WZProperty itemWz = stringWz.ResolveOutlink($"Item/Etc/{id.ToString("D8").Substring(0, 4)}.img/{id.ToString("D8")}");

            if (itemWz.Children.ContainsKey("info")) item.MetaInfo = ItemInfo.Parse(itemWz);
            item.Description = ItemDescription.Parse(stringWz, id);

            return item;
        }
    }
}
