using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PKG1;

namespace WZData.MapleStory.Mobs
{
    public class Mob
    {
        public const string WZFile = "Mob.wz";
        public const string FolderPath = "Mob";
        public const string StringPath = "Mob.img";
        public int Id;
        public MobMeta Meta;
        public string Name;
        public Dictionary<string, IEnumerable<FrameBook>> Framebooks;

        public static Mob Parse(WZProperty stringWz, bool followLink = true)
        {
            int id;

            if (!int.TryParse(stringWz.Name, out id)) return null;

            Mob result = new Mob();

            result.Id = id;

            WZProperty mobImage = stringWz.ResolveOutlink($"Mob/{id.ToString("D7")}") ?? stringWz.ResolveOutlink($"Mob2/{id.ToString("D7")}");

            result.Name = stringWz.ResolveForOrNull<string>("name");
            result.Meta = mobImage.Children.ContainsKey("info") ? MobMeta.Parse(mobImage.Resolve("info")) : null;
            /// Note: This *does* work. However, it increases the response to 1min+ and 15mb+
            /// Do *NOT* enable this unless people request it, and even then require an opt-in parameter.
            result.Framebooks = mobImage.Children
                .Where(c => c.Key != "info")
                .Where(c => c.Key == "fly" || c.Key == "stand")
                .ToDictionary(k => k.Key, v => FrameBook.Parse(v.Value));


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

        private void Extend(Mob linked)
            => this.Framebooks = linked.Framebooks;
    }
}
