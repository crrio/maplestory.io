using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using PKG1;

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
        public Dictionary<string, int> Framebooks;

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
    }
}
