using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using PKG1;
using WZData.MapleStory.Maps;
using WZData.MapleStory.Items;

namespace WZData.MapleStory.Mobs
{
    public class Mob
    {
        public const string WZFile = "Mob.wz";
        public const string FolderPath = "Mob";
        public const string StringPath = "Mob.img";
        public int Id;
        [JsonIgnore]
        public WZProperty mobImage { get; private set; }

        public int? LinksTo;
        public MobMeta Meta;
        public string Name;
        public string Description;
        public Dictionary<string, int> Framebooks;
        public MapName[] FoundAt;
        public ItemName[] Drops;
        [JsonIgnore]
        public Drop[] RealDrops;

        public static Mob Parse(WZProperty stringWz, bool followLink = true)
        {
            int id;

            if (!int.TryParse(stringWz.Name, out id)) return null;

            Mob result = new Mob();

            result.Id = id;

            result.mobImage = stringWz.ResolveOutlink($"Mob/{id.ToString("D7")}") ?? stringWz.ResolveOutlink($"Mob2/{id.ToString("D7")}");

            result.Name = stringWz.ResolveForOrNull<string>("name");
            result.Meta = result.mobImage.Children.ContainsKey("info") ? MobMeta.Parse(result.mobImage.Resolve("info")) : null;
            result.LinksTo = result.Meta.LinksToOtherMob;

            result.Framebooks = result.mobImage.Children
                .Where(c => c.Key != "info")
                .ToDictionary(c => c.Key, c => FrameBook.GetFrameCount(c.Value));

            WZProperty familiarEntry = stringWz.ResolveOutlink($"String/MonsterBook/{id}");
            result.Description = familiarEntry?.ResolveForOrNull<string>("episode");

            ILookup<int, MapName> lookup = MapName.GetMapNameLookup(stringWz);
            result.FoundAt = stringWz.ResolveOutlink($"Etc/MobLocation/{id}")?
                .Children.Values.Concat(familiarEntry?.Resolve("map")?.Children.Values ?? (new Dictionary<string, WZProperty>()).Values)
                .Select(c => c.ResolveFor<int>() ?? -1).Distinct()
                .Select(c => lookup[c]?.FirstOrDefault() ?? new MapName() { Name = "Unknown", StreetName = "Unknown", Id = c })
                .ToArray();

            ILookup<int, ItemNameInfo> reportedDrops = ItemNameInfo.GetNameLookup(stringWz.ResolveOutlink("String"));
            result.Drops = familiarEntry?.Resolve("reward")?.Children.Values
                .Select(c => c.ResolveFor<int>() ?? -1)
                .Select(c => reportedDrops[c]?.FirstOrDefault())
                .Where(c => c != null)
                .ToArray();

            List<int> linkFollowed = new List<int>();
            Mob linked = result;
            while (followLink && linked.Meta.LinksToOtherMob.HasValue && !linkFollowed.Contains(linked.Meta.LinksToOtherMob.Value)) {
                linkFollowed.Add(linked.Meta.LinksToOtherMob.Value);
                linked = Parse(stringWz.ResolveOutlink($"String/Mob/{linked.Meta.LinksToOtherMob.Value}"), false);
            }

            if (linked != result) {
                result.Extend(linked);
            }

            return result;
        }

        public IEnumerable<FrameBook> GetFrameBook(string bookName = null)
            => FrameBook.Parse(mobImage.Resolve(bookName ?? Framebooks.First().Key));

        private void Extend(Mob linked)
        {
            this.Framebooks = linked.Framebooks;
            this.mobImage = linked.mobImage;
        }

        public static Frame GetFirstFrame(WZProperty anyWz, int id)
            => GetFirstFrame(anyWz, id.ToString("D7"));

        public static Frame GetFirstFrame(WZProperty anyWz, string id)
        {
            WZProperty npcImg = anyWz.ResolveOutlink($"Mob/{id}");
            string linksTo = npcImg.ResolveForOrNull<string>("info/link");
            if (linksTo != null)
                return GetFirstFrame(anyWz, linksTo);
            return FrameBook.Parse(npcImg.Children.Where(c => c.Key != "info").Select(c => c.Value)?.FirstOrDefault())?
                    .FirstOrDefault()?.frames.FirstOrDefault();
        }

        public static string GetName(WZProperty anyWz, int id)
            => anyWz.ResolveOutlinkForOrNull<string>($"String/Mob/{id}/name");
    }
}
