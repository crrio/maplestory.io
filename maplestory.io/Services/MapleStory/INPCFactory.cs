using System.Collections.Generic;
using WZData.MapleStory.NPC;

namespace maplestory.io.Services.MapleStory
{
    public interface INPCFactory
    {
        NPC GetNPC(int id);
        IEnumerable<NPCInfo> GetNPCs();
    }
}
