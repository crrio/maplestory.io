using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PKG1;

namespace WZData.MapleStory.Items
{
    public class Install : MapleItem
    {
        public const string WZFile = "Item.wz";
        public const string FolderPath = "Install";
        public const bool IMGId = false;
        public const string StringPath = "Ins.img";

        public Install(int id) : base(id) { }

        public static Install Parse(WZProperty stringWz)
        {
            int id;

            if (!int.TryParse(stringWz.Name, out id)) return null;

            Install item = new Install(id);

            WZProperty itemWz = stringWz.ResolveOutlink($"Item/Install/{id.ToString("D8").Substring(0, 4)}.img/{id.ToString("D8")}");

            if (itemWz.Children.ContainsKey("info")) item.MetaInfo = ItemInfo.Parse(itemWz);
            item.Description = ItemDescription.Parse(stringWz, id);

            return item;
        }
    }
}
