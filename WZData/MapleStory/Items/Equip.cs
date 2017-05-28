using WZData.MapleStory.Images;
using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace WZData.MapleStory.Items
{
    public class Equip : MapleItem
    {
        public const string WZFile = "Item.wz";
        public const string FolderPath = "Etc";
        public const bool IMGId = false;
        public const string StringPath = "Eqp.img/Eqp";

        public Dictionary<string, EquipFrameBook> FrameBooks;

        public Equip(int id) : base(id) { FrameBooks = new Dictionary<string, EquipFrameBook>(); }

        public static Equip Parse(WZDirectory characterWz, WZObject stringItem, string group, int id, WZDirectory stringWz, bool showEffects = true)
        {
            Equip item = new Equip(id);
            try
            {
                WZObject characterItem = characterWz.ResolvePath(Path.Combine(group, $"{id.ToString("D8")}.img"));
                if (characterItem.HasChild("info")) item.MetaInfo = ItemInfo.Parse(characterWz, characterItem["info"]);
                item.Description = ItemDescription.Parse(stringItem, StringPath);
                bool isOnlyDefault = false;

                if (showEffects)
                {
                    foreach (WZObject obj in characterItem)
                    {
                        int frameTest = 0;
                        if (isOnlyDefault = (obj.Type == WZObjectType.Canvas || int.TryParse(obj.Name, out frameTest))) break;
                        if (obj.Name.Equals("info")) continue;

                        item.FrameBooks.Add(obj.Name, EquipFrameBook.Parse(characterWz, characterItem, obj));
                    }

                    if (isOnlyDefault)
                        item.FrameBooks.Add("default", EquipFrameBook.Parse(characterWz, characterItem, characterItem));
                }

                return item;
            } catch (Exception)
            {
                return null;
            }
        }

        public static Equip Search(WZDirectory characterWz, WZDirectory stringWz, int itemId)
        {
            int id = -1;
            foreach (WZObject idGrouping in stringWz.ResolvePath(StringPath))
                foreach (WZObject item in idGrouping)
                    if (int.TryParse(item.Name, out id) && id == itemId)
                    {
                        return Equip.Parse(characterWz, item, idGrouping.Name, id, stringWz);
                    }
            return null;
        }

        public static IEnumerable<Equip> Parse(WZDirectory characterWz, WZDirectory stringWz)
        {
            int id = -1;
            foreach (WZObject idGrouping in stringWz.ResolvePath(StringPath))
                foreach (WZObject item in idGrouping)
                    if (int.TryParse(item.Name, out id))
                        yield return Equip.Parse(characterWz, item, idGrouping.Name, id, stringWz);
        }

        public static IEnumerable<Tuple<int, Func<bool, MapleItem>>> GetLookup(WZDirectory characterWz, WZDirectory stringWz)
        {
            int id = -1;
            foreach (WZObject idGrouping in stringWz.ResolvePath(StringPath))
                foreach (WZObject item in idGrouping)
                    if (int.TryParse(item.Name, out id))
                        yield return new Tuple<int, Func<bool, MapleItem>>(id, CreateLookup(characterWz, item, idGrouping.Name, id, stringWz));
        }

        private static Func<bool, MapleItem> CreateLookup(WZDirectory characterWz, WZObject item, string idGroupingName, int id, WZDirectory stringWz) => (showEffects) => Equip.Parse(characterWz, item, idGroupingName, id, stringWz, showEffects);
    }
}
