using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WZData.MapleStory.NPC;

namespace maplestory.io.Services.MapleStory
{
    public class NPCFactory : INPCFactory
    {
        private Dictionary<int, Func<NPC>> NPCLookup;
        private NPCInfo[] allNPCMeta;

        public NPCFactory(IWZFactory factory)
        {
            Tuple<int, NPCInfo, Func<NPC>>[] mobs = NPC.GetLookup(factory.GetWZFile(WZ.Npc).MainDirectory, factory.GetWZFile(WZ.String).MainDirectory).ToArray();
            NPCLookup = mobs.ToDictionary(k => k.Item1, v => v.Item3);
            allNPCMeta = mobs.Select(c => c.Item2).ToArray();
        }
        public NPC GetNPC(int id) => NPCLookup[id]();
        public IEnumerable<NPCInfo> GetNPCs() => allNPCMeta;
    }
}
