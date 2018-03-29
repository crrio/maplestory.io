using System.Collections.Generic;
using PKG1;
using maplestory.io.Data;
using maplestory.io.Data.NPC;
using maplestory.io.Data.Images;

namespace maplestory.io.Services.Interfaces.MapleStory
{
    public interface INPCFactory
    {
        NPC GetNPC(int id);
        IEnumerable<NPCInfo> GetNPCs();
        IEnumerable<Frame> GetFrames(int npcId, string frameBook);
    }
}
