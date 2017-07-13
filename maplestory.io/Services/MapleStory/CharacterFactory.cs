using System;
using System.Collections.Generic;
using ImageSharp;
using System.Linq;
using System.Threading.Tasks;
using WZData;
using WZData.MapleStory;
using WZData.MapleStory.Characters;
using WZData.MapleStory.Items;
using System.IO.Compression;
using System.IO;
using WZData.MapleStory.Images;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace maplestory.io.Services.MapleStory
{
    public class CharacterFactory : ICharacterFactory
    {
        private readonly Dictionary<int, CharacterSkin> skins;
        private readonly IItemFactory itemFactory;
        private readonly ZMap zmap;
        private readonly SMap smap;
        private readonly ILogger<CharacterFactory> _logger;

        public CharacterFactory(IWZFactory factory, IItemFactory itemFactory, IZMapFactory zMapFactory, ILogger<CharacterFactory> logger)
        {
            _logger = logger;
            skins = CharacterSkin.Parse(factory.GetWZFile(WZ.Character).MainDirectory).ToDictionary(c => c.Id);
            zmap = zMapFactory.GetZMap();
            smap = zMapFactory.GetSMap();
            this.itemFactory = itemFactory;
        }

        public CharacterSkin GetSkin(int id) => skins[id];

        public int[] GetSkinIds() => skins.Keys.ToArray();

        public Image<Rgba32> GetBase(int id, string animation = null, int frame = 0, bool showEars = false, int padding = 2, string renderMode = "default")
            => GetCharacter(id, animation, frame, showEars, padding, renderMode, new Tuple<int, string>[0]);

        public Image<Rgba32> GetBaseWithHair(int id, string animation = null, int frame = 0, bool showEars = false, int padding = 2, int faceId = 20305, int hairId = 37831, string renderMode = "default")
            => GetCharacter(id, animation, frame, showEars, padding, renderMode, new Tuple<int, string>(faceId, null), new Tuple<int, string>(hairId, null));

        public Image<Rgba32> GetCharacter(int id, string animation = null, int frame = 0, bool showEars = false, int padding = 2, string renderMode = "default", params Tuple<int, string>[] itemEntries)
            => GetCharacter(id, animation, frame, showEars, padding, renderMode, itemEntries.Select(c => new Tuple<int, string, int?>(c.Item1, c.Item2, null)).ToArray());

        public Image<Rgba32> GetCharacter(int id, string animation = null, int frame = 0, bool showEars = false, int padding = 2, string renderMode = "default", params Tuple<int, string, int?>[] itemEntries)
        {
            Stopwatch watch;
            IEnumerable<EquipEntry> items = itemEntries.Select((c) => {
                Stopwatch loadWatch = Stopwatch.StartNew();
                var res = new EquipEntry(){
                    Equip = (Equip)itemFactory.search(c.Item1),
                    Action = c.Item2,
                    Frame = c.Item3
                };//(itemFactory.search(c.Item1), c.Item2, c.Item3);
                loadWatch.Stop();
                _logger.LogDebug($"Took {loadWatch.ElapsedMilliseconds}ms to load item {c.Item1}");
                return res;
            });

            watch = Stopwatch.StartNew();
            CharacterSkin skin = GetSkin(id);
            watch.Stop();
            _logger.LogDebug($"Took {watch.ElapsedMilliseconds}ms to load skin {id}");
            watch.Restart();
            CharacterAvatar avatar = new CharacterAvatar(skin);
            avatar.Items = items;

            if (animation == null)
            {
                Equip weapon = avatar.Equips.Where(c => c.EquipGroup == "Weapon").FirstOrDefault();
                animation = weapon?.FrameBooks.Select(c => c.Key).Where(c => c.Contains("stand")).FirstOrDefault() ?? "stand1";
            }

            avatar.AnimationName = animation;
            avatar.Frame = frame;
            avatar.ShowEars = showEars;
            avatar.Padding = padding;
            watch.Stop();
            _logger.LogDebug($"Took {watch.ElapsedMilliseconds}ms to initialize CharacterAvatar");
            watch.Restart();
            var result = avatar.Render(zmap, smap, renderMode);
            watch.Stop();
            _logger.LogDebug($"Took {watch.ElapsedMilliseconds}ms to render CharacterAvatar");
            return result;
        }

        public string[] GetActions(params int[] itemEntries)
        {
            Equip[] eqps = itemEntries.Select(itemFactory.search)
                .Where(c => c is Equip)
                .Select(c => (Equip)c)
                .Concat(new[] { (Equip)itemFactory.search(1040004) })
                .ToArray();

            return GetActions(eqps);
        }

        public string[] GetActions(params Equip[] eqps)
        {
            CharacterSkin skin = GetSkin(2000);
            eqps = eqps.Where(c => c.FrameBooks.ContainsKey("stand1") || c.FrameBooks.ContainsKey("stand2")).ToArray();

            return skin.Animations.Where(c => c.Value.AnimationName.Equals(c.Key, StringComparison.CurrentCultureIgnoreCase)).Select(c => c.Key).Where(c => eqps.All(e => e.FrameBooks.ContainsKey(c))).ToArray();
        }

        public byte[] GetSpriteSheet(int id, bool showEars = false, int padding = 2, string renderMode = "default", params int[] itemEntries)
        {
            Equip[] eqps = itemEntries
                .Select(itemFactory.search)
                .Where(c => c is Equip)
                .Select(c => (Equip)c)
                .ToArray();

            Equip face = eqps.Where(c => c.id >= 20000 && c.id <= 25000).FirstOrDefault();

            CharacterSkin skin = GetSkin(id);

            string[] actions = GetActions(eqps);

            using (MemoryStream mem = new MemoryStream())
            {
                using (ZipArchive archive = new ZipArchive(mem, ZipArchiveMode.Create, true))
                {
                    foreach (string emotion in face?.FrameBooks?.Keys?.ToArray() ?? new[] { "default" })
                    {
                        int emotionFrames = face?.FrameBooks[emotion]?.frames?.Count() ?? 1;
                        for (int emotionFrame = 0; emotionFrame < emotionFrames; ++emotionFrame)
                        {
                            foreach (string animation in actions)
                            {
                                for (int frame = 0; frame < skin.Animations[animation].Frames.Length; ++frame)
                                {
                                    ZipArchiveEntry entry = archive.CreateEntry($"{emotion}/{emotionFrame}/{animation}_{frame}.png", CompressionLevel.Optimal);
                                    using (Stream entryData = entry.Open())
                                    {
                                        Tuple<int, string, int?>[] items = itemEntries
                                            .Select(c => new Tuple<int, string, int?>(c, (c == face?.id) ? emotion : null, (c == face?.id) ? (int?)emotionFrame : null))
                                            .ToArray();
                                        Image<Rgba32> frameImage = GetCharacter(id, animation, frame, showEars, padding, renderMode, items);
                                        frameImage.SaveAsPng(entryData);

                                        entryData.Flush();
                                    }
                                }
                            }
                        }
                    }
                }

                return mem.ToArray();
            }
        }
    }
}
