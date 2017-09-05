using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using PKG1;

namespace WZData.MapleStory.NPC
{
    public class NPC
    {
        public Dictionary<string, string> Dialogue;

        [JsonIgnore]
        public WZProperty npcImg { get; private set; }

        public IEnumerable<string> KnownMessages;
        public string Function;
        public string Name;
        public bool IsShop;
        public List<string> Framebooks;
        public int Id;
        private int? Link;
        private bool? IsComponentNPC;
        private int? ComponentSkin;
        private int[] ComponentIds;

        public static NPC Parse(WZProperty stringWz, bool followLink = true)
        {
            int id;

            if (!int.TryParse(stringWz.Name, out id)) return null;

            NPC result = new NPC();
            result.Id = id;

            result.Function = stringWz.ResolveForOrNull<string>("func");
            result.Name = stringWz.ResolveForOrNull<string>("name");
            result.Dialogue = stringWz.Children
                .Where(c => c.Key != "func" && c.Key != "name" && c.Key != "dialogue")
                .ToDictionary(c => c.Key, c => ((IWZPropertyVal)c.Value).GetValue().ToString());

            result.npcImg = stringWz.ResolveOutlink($"Npc/{id.ToString("D7")}");

            result.IsShop = result.npcImg?.ResolveFor<bool>("info/shop") ?? false;
            result.Link = result.npcImg.ResolveFor<int>("link");

            result.Framebooks = result.npcImg.Children
                .Where(c => c.Key != "info")
                .Select(k => k.Key).ToList();
            if (result.npcImg.Resolve("info/default")?.Type == PropertyType.Canvas)
                result.Framebooks.Add("default");

            result.IsComponentNPC = result.npcImg.ResolveFor<bool>("info/componentNPC") ?? false;
            if (result.IsComponentNPC ?? false) {
                result.ComponentIds = result.npcImg.Resolve("info/component")?.Children.Where(c => c.Key != "skin").Select(c => c.Value.ResolveFor<int>()).Where(c => c.HasValue).Select(c => c.Value).ToArray();
                result.ComponentSkin = result.npcImg.ResolveFor<int>("info/component/skin") + 2000;
            }

            List<int> linkFollowed = new List<int>();
            NPC linked = result;
            while (followLink && linked.Link.HasValue && !linkFollowed.Contains(linked.Link.Value)) {
                linkFollowed.Add(linked.Link.Value);
                linked = Parse(stringWz.ResolveOutlink($"String/Npc/{linked.Link.Value}"), false);
            }

            if (linked != result) {
                result.Extend(linked);
            }

            return result;
        }

        public IEnumerable<FrameBook> GetFrameBook(string bookName = null)
            => FrameBook.Parse(npcImg.Resolve(bookName ?? Framebooks.First()));

        private void Extend(NPC linked)
            => this.Framebooks = linked.Framebooks;
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
