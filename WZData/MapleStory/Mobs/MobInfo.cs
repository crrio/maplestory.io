﻿using System;
using System.Collections.Generic;
using System.Text;

namespace WZData.MapleStory.Mobs
{
    public class NPCInfo
    {
        public int Id;
        public string Name;

        public NPCInfo(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}