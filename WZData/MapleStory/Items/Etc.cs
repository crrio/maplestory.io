using reWZ;
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

        public static Etc Parse(WZDirectory itemWz, WZObject itemStringEntry, int id, WZDirectory stringWz)
        {
            Etc item = new Etc(id);

            WZObject itemWzEntry = null;
            try
            {
                itemWzEntry = itemWz.ResolvePath($"Etc/{id.ToString("D8").Substring(0, 4)}.img/{id.ToString("D8")}");
            }
            catch (Exception) { return null; }

            if (itemWzEntry.HasChild("info")) item.MetaInfo = ItemInfo.Parse(itemWz, itemWzEntry["info"]);
            item.Description = ItemDescription.Parse(itemStringEntry, StringPath);

            return item;
        }

        public static IEnumerable<Tuple<int, Func<MapleItem>>> GetLookup(Func<Func<WZFile, MapleItem>, MapleItem> itemWzCallback, WZDirectory stringWz)
        {
            int id = -1;
            foreach(WZObject item in stringWz.ResolvePath(StringPath))
                    if (int.TryParse(item.Name, out id))
                        yield return new Tuple<int, Func<MapleItem>>(id, CreateLookup(itemWzCallback, item, id, stringWz).Memoize());
        }

        private static Func<MapleItem> CreateLookup(Func<Func<WZFile, MapleItem>, MapleItem> itemWzCallback, WZObject item, int id, WZDirectory stringWz)
            => ()
            => itemWzCallback(itemWz => Etc.Parse(itemWz.MainDirectory, item, id, stringWz));
    }
}
