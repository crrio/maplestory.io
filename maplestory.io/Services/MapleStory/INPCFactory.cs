using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WZData.MapleStory.Mob;

namespace maplestory.io.Services.MapleStory
{
    public interface INPCFactory
    {
        NPC GetNPC(int id);
        IEnumerable<NPCInfo> GetNPCs();
    }
}
