using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WZData.MapleStory.Items
{
    public class Cash : MapleItem
    {
        public const string WZFile = "Item.wz";
        public const string FolderPath = "Cash";
        public const bool IMGId = false;
        public const string StringPath = "Cash.img";

        public CashEffect effect;

        public Cash(int id) : base(id) { }

        public static Cash Parse(WZDirectory itemWz, WZObject cashItem, int id, WZDirectory stringWz, bool showEffects = true)
        {
            Cash item = new Cash(id);

            if (cashItem.HasChild("info")) item.MetaInfo = ItemInfo.Parse(itemWz, cashItem["info"]);
            if (cashItem.HasChild("effect") && showEffects) item.effect = CashEffect.Parse(itemWz, cashItem, cashItem["effect"]);

            try
            {
                WZObject stringInfo = stringWz.ResolvePath(Path.Combine(StringPath, id.ToString()));
                item.Description = ItemDescription.Parse(stringInfo, StringPath);
            }
            catch (Exception ex)
            {
                // Sometimes they just don't have names :/
            }

            return item;
        }

        public static IEnumerable<Cash> Parse(WZDirectory itemWz, WZDirectory stringWz)
        {
            int id = -1;
            foreach (WZObject idGrouping in itemWz.ResolvePath(FolderPath))
                foreach (WZObject item in idGrouping)
                    if (int.TryParse(item.Name, out id))
                        yield return Cash.Parse(itemWz, item, id, stringWz);
        }

        public static IEnumerable<Tuple<int, Func<MapleItem>>> GetLookup(WZDirectory itemWz, WZDirectory stringWz)
        {
            int id = -1;
            foreach (WZObject idGrouping in itemWz.ResolvePath(FolderPath))
                foreach (WZObject item in idGrouping)
                    if (int.TryParse(item.Name, out id))
                        yield return new Tuple<int, Func<MapleItem>>(id, CreateLookup(itemWz, item, id, stringWz).Memoize());
        }

        private static Func<MapleItem> CreateLookup(WZDirectory itemWz, WZObject item, int id, WZDirectory stringWz)
            => ()
            => Cash.Parse(itemWz, item, id, stringWz, true);
    }
}
