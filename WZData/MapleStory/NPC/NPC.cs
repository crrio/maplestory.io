using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using PKG1;
using WZData.MapleStory.Maps;

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
        public Dictionary<string, int> Framebooks;
        public int Id;
        public int? Link;
        public bool? IsComponentNPC;
        public int? ComponentSkin;
        public int[] ComponentIds;
        public MapName[] FoundAt;

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
                .ToDictionary(c => c.Key, c => FrameBook.GetFrameCount(c.Value));

            if (result.npcImg.Resolve("info/default")?.Type == PropertyType.Canvas)
                result.Framebooks.Add("default", FrameBook.GetFrameCount(result.npcImg.Resolve("info/default")));

            result.IsComponentNPC = result.npcImg.ResolveFor<bool>("info/componentNPC") ?? false;
            if (result.IsComponentNPC ?? false) {
                result.ComponentIds = result.npcImg.Resolve("info/component")?.Children.Where(c => c.Key != "skin").Select(c => c.Value.ResolveFor<int>()).Where(c => c.HasValue).Select(c => c.Value).ToArray();
                result.ComponentSkin = result.npcImg.ResolveFor<int>("info/component/skin") + 2000;
            }

            result.FoundAt = result.npcImg.ResolveOutlink($"Etc/NpcLocation/{id}")
                .Children.Keys.Where(c => int.TryParse(c, out int blah))
                .Select(c => MapName.GetMapNameLookup(result.npcImg)[int.Parse(c)].FirstOrDefault())
                .Where(c => c != null).ToArray();

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
            => bookName == null && Framebooks.Count == 0 ? null : FrameBook.Parse(npcImg.Resolve(bookName ?? Framebooks.First().Key));

        private void Extend(NPC linked) {
            this.Framebooks = linked.Framebooks;
            this.npcImg = linked.npcImg;
        }

        public static Frame GetFirstFrame(WZProperty anyWz, int id)
            => GetFirstFrame(anyWz, id.ToString("D7"));

        public static Frame GetFirstFrame(WZProperty anyWz, string id)
        {
            WZProperty npcImg = anyWz.ResolveOutlink($"Npc/{id}");
            string linksTo = npcImg.ResolveForOrNull<string>("info/link");
            if (linksTo != null)
                return GetFirstFrame(anyWz, linksTo);
            return FrameBook.Parse(npcImg.Children.Where(c => c.Key != "info").Select(c => c.Value).FirstOrDefault())
                    .FirstOrDefault().frames.FirstOrDefault();
        }

        public static string GetName(WZProperty anyWz, int id)
            => anyWz.ResolveOutlinkForOrNull<string>($"String/Npc/{id}/name");
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
