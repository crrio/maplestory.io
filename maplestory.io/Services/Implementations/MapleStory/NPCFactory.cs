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
        public IEnumerable<NPCInfo> GetNPCs(int startAt, int count, string filter)
            => WZ.Resolve("String/Npc").Children.Select(NPCInfo.Parse).Where(c => string.IsNullOrEmpty(filter) || c.Name.Contains(filter)).Skip(startAt).Take(count);
        public IEnumerable<Frame> GetFrames(int npcId, string frameBook) => GetNPC(npcId)?.GetFrameBook(frameBook)?.First().frames;
    }
}
