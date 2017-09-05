using System.Collections.Generic;
using PKG1;
using WZData;
using WZData.MapleStory.NPC;

namespace maplestory.io.Services.MapleStory
{
    public interface INPCFactory : INeedWZ<INPCFactory>
    {
        NPC GetNPC(int id);
        IEnumerable<NPCInfo> GetNPCs();
        IEnumerable<Frame> GetFrames(int npcId, string frameBook);
    }
}
