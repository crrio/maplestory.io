 using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WZData.MapleStory;

namespace WZData
{
    public class MapleItem
    {
        public int id;
        public ItemDescription Description;
        public ItemInfo MetaInfo;
        public ItemType TypeInfo;

        public MapleItem(int id)
        {
            this.id = id;
            TypeInfo = ItemType.FindCategory(id);
        }
    }
}
