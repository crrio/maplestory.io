using maplestory.io.Data.Images;
using maplestory.io.Data.Mobs;
using System.Collections.Generic;

namespace maplestory.io.Services.Interfaces.MapleStory
{
    public interface IMobFactory
    {
        Mob GetMob(int id);
        IEnumerable<Frame> GetFrames(int mobId, string frameBook);
        IEnumerable<MobInfo> GetMobs(int startPosition = 0, int? count = null, int? minLevelFilter = null, int? maxLevelFilter = null, string searchFor = null);
    }
}
