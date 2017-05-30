using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WZData.MapleStory.Mobs;

namespace maplestory.io.Services.MapleStory
{
    public interface IMobFactory
    {
        Mob GetMob(int id);
        IEnumerable<MobInfo> GetMobs();
    }
}
