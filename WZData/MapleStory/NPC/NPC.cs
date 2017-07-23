using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PKG1;

namespace WZData.MapleStory.NPC
{
    public class NPC
    {
        public Dictionary<string, string> dialogue;
        public IEnumerable<string> KnownMessages;
        public string Function;
        public string Name;
        public bool IsShop;
        public Dictionary<string, IEnumerable<FrameBook>> Framebooks;
        public int Id;

        public static NPC Parse(WZProperty stringWz)
        {
            int id;

            if (!int.TryParse(stringWz.Name, out id)) return null;

            NPC result = new NPC();
            result.Id = id;

            result.Function = stringWz.ResolveForOrNull<string>("func");
            result.Name = stringWz.ResolveForOrNull<string>("name");
            result.dialogue = stringWz.Children
                .Where(c => c.Key != "func" && c.Key != "name" && c.Key != "dialogue")
                .ToDictionary(c => c.Key, c => ((IWZPropertyVal)c.Value).GetValue().ToString());

            WZProperty npcImg = stringWz.ResolveOutlink($"Npc/{id.ToString("D7")}");

            result.IsShop = npcImg?.ResolveFor<bool>("info/shop") ?? false;

            result.Framebooks = npcImg.Children
                //.Where(c => c.Key != "info") // We're only showing stand for now
                .Where(c => c.Key == "stand")
                .ToDictionary(k => k.Key, v => FrameBook.Parse(v.Value));

            return result;
        }
    }

    public class NPCInfo
    {
        public string Name;
        public int id;

        public NPCInfo(int id, string name)
        {
            this.id = id;
            this.Name = name;
        }

        public static NPCInfo Parse(WZProperty value)
            => new NPCInfo(
                int.Parse(value.Name),
                value.ResolveForOrNull<string>("name")
            );
    }
}
