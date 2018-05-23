using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;
using maplestory.io.Models;

namespace maplestory.io.Services.Interfaces.MapleStory
{
    public interface IWZFactory
    {
        MSPackageCollection GetWZ(Region region, string version);
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
