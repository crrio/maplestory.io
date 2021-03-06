﻿using maplestory.io.Data.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace maplestory.io.Data
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
