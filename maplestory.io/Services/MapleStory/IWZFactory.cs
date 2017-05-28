﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using reWZ;

namespace maplestory.io.Services.MapleStory
{
    public interface IWZFactory
    {
        WZFile GetWZFile(WZ file);
    }

    public enum WZ {
        Base,
        Character,
        Data,
        Effect,
        Etc,
        Item,
        Map,
        Map2,
        Mob,
        Mob2,
        Morph,
        Npc,
        Quest,
        Reactor,
        Skill,
        Sound,
        String,
        TamingMob,
        UI
    }
}