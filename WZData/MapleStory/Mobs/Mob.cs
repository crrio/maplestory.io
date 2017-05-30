using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public static Mob Parse(WZObject mobImage, WZDirectory mobWz, string name)
        {
            Mob result = new Mob();

            result.Id = int.Parse(mobImage.Name.Replace(".img", ""));
            result.Name = name;
            result.Meta = mobImage.HasChild("info") ? MobMeta.Parse(mobImage["info"]) : null;
            /// Note: This *does* work. However, it increases the response to 1min+ and 15mb+
            /// Do *NOT* enable this unless people request it, and even then require an opt-in parameter.
            result.Framebooks = mobImage
                .Where(c => c.Name != "info")
                .Where(c => c.Name == "fly" || c.Name == "stand")
                .Select(c => new Tuple<string, IEnumerable<FrameBook>>(c.Name, FrameBook.Parse(mobWz, mobImage, c)))
                .ToDictionary(k => k.Item1, v => v.Item2);

            return result;
        }

        public static IEnumerable<Tuple<int, MobInfo, Func<Mob>>> GetLookup(WZDirectory mobWz, WZDirectory stringWz)
        {
            int id = -1;
            foreach (WZObject entry in stringWz.ResolvePath(StringPath))
            {
                if (int.TryParse(entry.Name, out id) && entry.HasChild("name") && mobWz.HasChild($"{id.ToString("D7")}.img"))
                {
                    string name = entry["name"].ValueOrDefault<string>("");
                    yield return new Tuple<int, MobInfo, Func<Mob>>(id, new MobInfo(id, name), CreateLookup(mobWz.ResolvePath($"{id.ToString("D7")}.img"), mobWz, name));
                }
            }
        }

        private static Func<Mob> CreateLookup(WZObject mobImage, WZDirectory mobWZ, string MobName)
            => () 
            => Mob.Parse(mobImage, mobWZ, MobName);
    }
}
