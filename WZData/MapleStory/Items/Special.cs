using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;

namespace WZData.MapleStory.Items
{
    public class Special : MapleItem
    {
        public Special(int id) : base(id) { }
        public static Special Parse(WZProperty stringWz)
        {
            int id;

            if (!int.TryParse(stringWz.Name, out id)) return null;
            Special item = new Special(id);
            WZProperty specialWz = stringWz.ResolveOutlink($"Item/Special/{id.ToString("D8").Substring(0, 4)}.img/{id.ToString("D8")}");

            item.MetaInfo = ItemInfo.Parse(specialWz);
            item.Description = ItemDescription.Parse(stringWz, id);

            return item;
        }
    }
}
