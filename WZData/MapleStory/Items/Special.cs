using reWZ;
using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WZData.MapleStory.Items
{
    public class Special : MapleItem
    {
        public const string WZFile = "Item.wz";
        public const string FolderPath = "Special";
        public const bool IMGId = false;
        public const string StringPath = null;

        public Special(int id) : base(id)
        {

        }

        public static Special Parse(WZDirectory itemWz, WZObject specialItem, int id)
        {
            Special item = new Special(id);

            item.MetaInfo = ItemInfo.Parse(itemWz, specialItem);
            item.Description = ItemDescription.Parse(itemWz, FolderPath);

            return item;
        }

        public static IEnumerable<Special> Parse(WZDirectory itemWz)
        {
            int id = -1;
            foreach (WZObject idGrouping in itemWz.ResolvePath(FolderPath))
                foreach (WZObject item in idGrouping)
                    if (int.TryParse(item.Name, out id))
                        yield return Special.Parse(itemWz, item, id);
        }
    }
}
