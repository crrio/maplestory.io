using System;
using System.Collections.Generic;
using System.Text;

namespace WZData.MapleStory.Mobs
{
    public class MobInfo
    {
        public int Id;
        public string Name;

        public MobInfo(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
