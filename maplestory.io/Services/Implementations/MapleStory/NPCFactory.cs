using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;
using maplestory.io.Data;
using maplestory.io.Data.NPC;
using maplestory.io.Data.Images;
using maplestory.io.Services.Interfaces.MapleStory;

namespace maplestory.io.Services.Implementations.MapleStory
{
    public class NPCFactory : NeedWZ, INPCFactory
    {
        public NPC GetNPC(int id)
            => NPC.Parse(WZ.Resolve($"String/Npc/{id}"));
        public IEnumerable<NPCInfo> GetNPCs()
            => WZ.Resolve("String/Npc").Children.Select(NPCInfo.Parse);
        public IEnumerable<Frame> GetFrames(int npcId, string frameBook) => GetNPC(npcId)?.GetFrameBook(frameBook)?.First().frames;
    }
}
