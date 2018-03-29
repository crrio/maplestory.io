using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;
using maplestory.io.Data;
using maplestory.io.Data.Mobs;
using maplestory.io.Data.Images;

namespace maplestory.io.Services.Interfaces.MapleStory
{
    public interface IMobFactory
    {
        Mob GetMob(int id);
        IEnumerable<MobInfo> GetMobs();
        IEnumerable<Frame> GetFrames(int mobId, string frameBook);
    }
}
