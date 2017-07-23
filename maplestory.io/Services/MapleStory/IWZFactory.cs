﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;

namespace maplestory.io.Services.MapleStory
{
    public interface IWZFactory
    {
        PackageCollection GetWZ(Region region, string version);
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
