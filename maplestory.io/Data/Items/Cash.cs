using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PKG1;

namespace maplestory.io.Data.Items
{
    public class Cash : MapleItem
    {
        public CashEffect effect;
        public Cash(int id) : base(id) { }
        public static Cash Parse(WZProperty stringWz)
        {
            int id;

            if (!int.TryParse(stringWz.NameWithoutExtension, out id)) return null;

            Cash item = new Cash(id);

            WZProperty itemWz = stringWz.ResolveOutlink($"Item/Cash/{id.ToString("D8").Substring(0, 4)}.img/{id.ToString("D8")}");

            if (itemWz != null) {
                item.MetaInfo = ItemInfo.Parse(itemWz);
                item.effect = CashEffect.Parse((itemWz?.Resolve("effect") ?? itemWz));
            }
            item.Description = ItemDescription.Parse(stringWz, id);

            return item;
        }
    }
}
