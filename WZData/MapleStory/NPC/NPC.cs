using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WZData.MapleStory.NPC
{
    public class NPC
    {
        public const string WZFile = "Npc.wz";
        public const string StringPath = "Npc.img";

        public Dictionary<string, string> dialog;
        public string Function;
        public string Name;
        public bool IsShop;
        public Dictionary<string, IEnumerable<FrameBook>> Framebooks;

        public static NPC Parse(WZDirectory stringWz, WZObject npcInString, WZDirectory NPCwz, WZObject npcInNPC)
        {
            NPC result = new NPC();

            result.Function = npcInString.HasChild("func") ? npcInString["func"].ValueOrDefault<string>(null) : null;
            result.Name = npcInString.HasChild("name") ? npcInString["name"].ValueOrDefault<string>(null) : null;
            result.dialog = npcInString
                .Where(c => c.Name != "func" && c.Name != "name")
                .Select(c => new Tuple<string, string>(c.Name, c.ValueOrDefault<string>(null)))
                .Where(c => c.Item1 != null && c.Item2 != null)
                .ToDictionary(n => n.Item1, n => n.Item2);

            result.IsShop = npcInNPC.HasChild("info") && npcInNPC["info"].HasChild("shop") && npcInNPC.ResolvePath("info/shop").ValueOrDefault<int>(0) == 1;

            result.Framebooks = npcInNPC
                .Where(c => c.Name != "info")
                .Where(c => c.Name == "stand")
                .Select(c => new Tuple<string, IEnumerable<FrameBook>>(c.Name, FrameBook.Parse(NPCwz, npcInNPC, c)))
                .ToDictionary(k => k.Item1, v => v.Item2);

            return result;
        }

        public static IEnumerable<Tuple<int, NPCInfo, Func<NPC>>> GetLookup(WZDirectory npcWz, WZDirectory stringWz)
        {
            foreach(WZObject npcInString in stringWz.ResolvePath(StringPath))
            {
                int id = -1;
                if (int.TryParse(npcInString.Name, out id) && npcInString.HasChild("name"))
                {
                    string name = npcInString["name"].ValueOrDefault<string>("");
                    WZObject npcInNPC = npcWz.HasChild($"{id.ToString("D7")}.img") ? npcWz[$"{id.ToString("D7")}.img"] : null;
                    yield return new Tuple<int, NPCInfo, Func<NPC>>(id, new NPCInfo(id, name) , CreateLookup(stringWz, npcInString, npcWz, npcInNPC));
                }
            }
        }

        private static Func<NPC> CreateLookup(WZDirectory stringWz, WZObject npcInString, WZDirectory NPCwz, WZObject npcInNPC)
            => ()
            => NPC.Parse(stringWz, npcInString, NPCwz, npcInNPC);
    }

    public class NPCInfo
    {
        public string name;
        public int id;

        public NPCInfo(int id, string name)
        {
            this.id = id;
            this.name = name;
        }
    }
}
