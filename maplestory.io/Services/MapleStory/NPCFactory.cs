using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WZData.MapleStory.Mob;

namespace maplestory.io.Services.MapleStory
{
    public class NPCFactory : INPCFactory
    {
        private Dictionary<int, Func<NPC>> npcLookup;
        private NPCInfo[] allNPCMeta;

        public NPCFactory(IWZFactory factory)
        {
            Tuple<int, NPCInfo, Func<NPC>>[] npcs = NPC.GetLookup(factory.GetWZFile(WZ.Mob).MainDirectory, factory.GetWZFile(WZ.String).MainDirectory).ToArray();
            npcLookup = npcs.ToDictionary(k => k.Item1, v => v.Item3);
            allNPCMeta = npcs.Select(c => c.Item2).ToArray();
        }
        public NPC GetNPC(int id) => npcLookup[id]();

        public IEnumerable<NPCInfo> GetNPCs() => allNPCMeta;
    }
}
