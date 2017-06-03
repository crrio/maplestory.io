using System;
using System.Collections.Generic;
using System.Text;

namespace WZData.MapleStory.Items
{
    public class ItemName
    {
        public string Name, Desc;
        public int Id;
        public ItemType TypeInfo { get => ItemType.FindCategory(Id); }
    }
}