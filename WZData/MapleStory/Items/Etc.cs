using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WZData.MapleStory.Items
{
    public class Etc : MapleItem
    {
        public const string WZFile = "Item.wz";
        public const string FolderPath = "Etc";
        public const bool IMGId = false;
        public const string StringPath = "Etc.img/Etc";

        public Etc(int id) : base(id) { }

        public static Etc Parse(WZDirectory itemWz, WZObject cashItem, int id, WZDirectory stringWz)
        {
            Etc item = new Etc(id);

            if (cashItem.HasChild("info")) item.MetaInfo = ItemInfo.Parse(itemWz, cashItem["info"]);

            try
            {
                WZObject stringInfo = stringWz.ResolvePath(Path.Combine(StringPath, id.ToString()));
                item.Description = ItemDescription.Parse(stringInfo, StringPath);
            }
            catch (Exception)
            {
                // Sometimes they just don't have a name for items :/
            }

            return item;
        }

        public static IEnumerable<Etc> Parse(WZDirectory itemWz, WZDirectory stringWz)
        {
            int id = -1;
            foreach (WZObject idGrouping in itemWz.ResolvePath(FolderPath))
                foreach (WZObject item in idGrouping)
                    if (int.TryParse(item.Name, out id))
                        yield return Etc.Parse(itemWz, item, id, stringWz);
        }

        public static IEnumerable<Tuple<int, Func<MapleItem>>> GetLookup(WZDirectory itemWz, WZDirectory stringWz)
        {
            int id = -1;
            foreach (WZObject idGrouping in itemWz.ResolvePath(FolderPath))
                foreach (WZObject item in idGrouping)
                    if (int.TryParse(item.Name, out id))
                        yield return new Tuple<int, Func<MapleItem>>(id, CreateLookup(itemWz, item, id, stringWz));
        }

        private static Func<MapleItem> CreateLookup(WZDirectory itemWz, WZObject item, int id, WZDirectory stringWz)
            => ()
            => Etc.Parse(itemWz, item, id, stringWz);
    }
}
