using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;
using WZData;
using WZData.MapleStory.NPC;
using WZData.MapleStory.Images;

namespace maplestory.io.Services.MapleStory
{
    public class NPCFactory : NeedWZ<INPCFactory>, INPCFactory
    {
        public NPCFactory(IWZFactory factory) : base(factory) { }
        public NPCFactory(IWZFactory factory, Region region, string version) : base(factory, region, version) { }

        public NPC GetNPC(int id)
            => NPC.Parse(wz.Resolve($"String/Npc/{id}"));
        public IEnumerable<NPCInfo> GetNPCs()
            => wz.Resolve("String/Npc").Children.Values.Select(NPCInfo.Parse);
        public IEnumerable<Frame> GetFrames(int npcId, string frameBook) => GetNPC(npcId)?.GetFrameBook(frameBook)?.First().frames;

        public override INPCFactory GetWithWZ(Region region, string version)
            => new NPCFactory(_factory, region, version);
    }
}
