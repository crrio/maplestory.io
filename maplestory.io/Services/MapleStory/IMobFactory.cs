using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;
using WZData;
using WZData.MapleStory.Mobs;

namespace maplestory.io.Services.MapleStory
{
    public interface IMobFactory : INeedWZ<IMobFactory>
    {
        Mob GetMob(int id);
        IEnumerable<MobInfo> GetMobs();
        IEnumerable<Frame> GetFrames(int mobId, string frameBook);
    }
}
