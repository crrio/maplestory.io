﻿using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WZData.MapleStory.Items
{
    public class Consume : MapleItem
    {
        public const string WZFile = "Item.wz";
        public const string FolderPath = "Consume";
        public const bool IMGId = false;
        public const string StringPath = "Consume.img";

        public Consume(int id) : base(id) { }

        public static Consume Parse(WZDirectory itemWz, WZObject cashItem, int id, WZDirectory stringWz)
        {
            Consume item = new Consume(id);

            if (cashItem.HasChild("info")) item.MetaInfo = ItemInfo.Parse(itemWz, cashItem["info"]);

            try
            {
                WZObject stringInfo = stringWz.ResolvePath(Path.Combine(StringPath, id.ToString()));
                item.Description = ItemDescription.Parse(stringInfo, StringPath);
            } catch (Exception ex)
            {
                // Some items just don't have a name. :/
            }

            return item;
        }

        public static IEnumerable<Consume> Parse(WZDirectory itemWz, WZDirectory stringWz)
        {
            int id = -1;
            foreach (WZObject idGrouping in itemWz.ResolvePath(FolderPath))
                foreach (WZObject item in idGrouping)
                    if (int.TryParse(item.Name, out id))
                        yield return Consume.Parse(itemWz, item, id, stringWz);
        }

        public static IEnumerable<Tuple<int, Func<MapleItem>>> GetLookup(WZDirectory itemWz, WZDirectory stringWz)
        {
            int id = -1;
            foreach (WZObject idGrouping in itemWz.ResolvePath(FolderPath))
                foreach (WZObject item in idGrouping)
                    if (int.TryParse(item.Name, out id))
                        yield return new Tuple<int, Func<MapleItem>>(id, CreateLookup(itemWz, item, idGrouping.Name, id, stringWz));
        }

        private static Func<MapleItem> CreateLookup(WZDirectory itemWz, WZObject item, string idGroupingName, int id, WZDirectory stringWz)
            => () 
            => Consume.Parse(itemWz, item, id, stringWz);
    }
}