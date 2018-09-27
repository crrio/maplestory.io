using maplestory.io.Data.Images;
using maplestory.io.Data.Maps;
using maplestory.io.Data.Quests;
using maplestory.io.Models;
using Newtonsoft.Json;
using PKG1;
using System.Collections.Generic;
using System.Linq;

namespace maplestory.io.Data.NPC
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
        public int[] RelatedQuests;
        public QuestName[] RelatedQuestsInfo;

        public static NPC Parse(WZProperty stringWz, bool followLink = true)
        {
            int id;

            if (!int.TryParse(stringWz.NameWithoutExtension, out id)) return null;

            NPC result = new NPC();
            result.Id = id;

            if (stringWz?.FileContainer?.Collection is MSPackageCollection)
                ((MSPackageCollection)stringWz.FileContainer.Collection).NPCQuests.TryGetValue(id, out result.RelatedQuests);

            result.npcImg = stringWz.ResolveOutlink($"Npc/{id.ToString("D7")}");
            result.Link = result.npcImg.ResolveFor<int>("info/link") ?? result.npcImg.ResolveFor<int>("link");
            List<int> linkFollowed = new List<int>();
            NPC linked = result;
            while (followLink && linked.Link.HasValue && !linkFollowed.Contains(linked.Link.Value))
            {
                linkFollowed.Add(linked.Link.Value);
                linked = Parse(stringWz.ResolveOutlink($"String/Npc/{linked.Link.Value}"), false);
            }

            if (linked != result)
            {
                result.Extend(linked);
            }

            result.Function = stringWz.ResolveForOrNull<string>("func");
            result.Name = stringWz.ResolveForOrNull<string>("name");
            result.Dialogue = stringWz.Children
                .Where(c => c.NameWithoutExtension != "func" && c.NameWithoutExtension != "name" && c.NameWithoutExtension != "dialogue" && c is IWZPropertyVal)
                .ToDictionary(c => c.NameWithoutExtension, c => ((IWZPropertyVal)c).GetValue().ToString());

            result.IsShop = result.npcImg?.ResolveFor<bool>("info/shop") ?? false;

            result.Framebooks = result.npcImg.Children
                .Where(c => c.NameWithoutExtension != "info")
                .ToDictionary(c => c.NameWithoutExtension, c => FrameBook.GetFrameCount(c));

            if (result.npcImg.Resolve("info/default")?.Type == PropertyType.Canvas)
                result.Framebooks.Add("default", FrameBook.GetFrameCount(result.npcImg.Resolve("info/default")));

            result.IsComponentNPC = result.npcImg.ResolveFor<bool>("info/componentNPC") ?? false;
            if (result.IsComponentNPC ?? false) {
                result.ComponentIds = result.npcImg.Resolve("info/component")?.Children.Where(c => c.NameWithoutExtension  != "skin").Select(c => c.ResolveFor<int>()).Where(c => c.HasValue).Select(c => c.Value).ToArray();
                result.ComponentSkin = result.npcImg.ResolveFor<int>("info/component/skin") + 2000;
            }

            result.FoundAt = result.npcImg.ResolveOutlink($"Etc/NpcLocation/{id}")?
                .Children.Where(c => int.TryParse(c.NameWithoutExtension, out int blah))
                .Select(c => MapName.GetMapNameLookup(result.npcImg)[int.Parse(c.NameWithoutExtension)].FirstOrDefault())
                .Where(c => c != null).ToArray();

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
            if (npcImg == null) return null;
            string linksTo = npcImg.ResolveForOrNull<string>("info/link");
            if (linksTo != null)
                return GetFirstFrame(anyWz, linksTo);
            return FrameBook.Parse(npcImg.Children.Where(c => c.NameWithoutExtension != "info").Select(c => c).FirstOrDefault())
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
                int.Parse(value.NameWithoutExtension),
                value.ResolveForOrNull<string>("name")
            );
    }
}
