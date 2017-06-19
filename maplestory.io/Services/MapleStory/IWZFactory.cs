using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using reWZ;
using WZData;
using WZData.MapleStory;

namespace maplestory.io.Services.MapleStory
{
    public interface IWZFactory
    {
        WZFile GetWZFile(WZ file, WZLanguage language = WZLanguage.English);
        Func<Func<WZFile, MapleItem>, MapleItem> AsyncGetWZFile(WZ file, WZLanguage language = WZLanguage.English);
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
