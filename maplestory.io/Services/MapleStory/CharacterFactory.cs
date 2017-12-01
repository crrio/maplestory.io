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
using PKG1;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;

namespace maplestory.io.Services.MapleStory
{
    public class CharacterFactory : NeedWZ<ICharacterFactory>, ICharacterFactory
    {
        private readonly IItemFactory itemFactory;
        private readonly ZMap zmap;
        private readonly SMap smap;
        private readonly ILogger<CharacterFactory> _logger;
        private readonly IZMapFactory _zmapFactory;

        public CharacterFactory(IWZFactory factory, IItemFactory itemFactory, IZMapFactory zMapFactory, ILogger<CharacterFactory> logger, Region region, string version)
            : base (factory, region, version) {
            _logger = logger;
            _zmapFactory = zMapFactory;
            zmap = _zmapFactory.GetZMap();
            smap = _zmapFactory.GetSMap();
            this.itemFactory = itemFactory;
        }
        public CharacterFactory(IWZFactory factory, IItemFactory itemFactory, IZMapFactory zMapFactory, ILogger<CharacterFactory> logger)
            : base(factory)
        {
            _logger = logger;
            _zmapFactory = zMapFactory;
            this.itemFactory = itemFactory;
        }

        public CharacterSkin GetSkin(int id)
            => CharacterSkin.Parse(wz.Resolve("Character"), id);

        public int[] GetSkinIds()
            => CharacterSkin.Parse(wz.Resolve("Character")).Select(c => c.Id).ToArray();

        public Image<Rgba32> GetBase(int id, string animation = null, int frame = 0, bool showEars = false, bool showLefEars = false, int padding = 2, RenderMode renderMode = RenderMode.Full)
            => GetCharacter(id, animation, frame, showEars, showLefEars, padding, renderMode, new Tuple<int, string>[0]);

        public Image<Rgba32> GetBaseWithHair(int id, string animation = null, int frame = 0, bool showEars = false, bool showLefEars = false, int padding = 2, int faceId = 20305, int hairId = 37831, RenderMode renderMode = RenderMode.Full)
            => GetCharacter(id, animation, frame, showEars, showLefEars, padding, renderMode, new Tuple<int, string>(faceId, null), new Tuple<int, string>(hairId, null));

        public Image<Rgba32> GetCharacter(int id, string animation = null, int frame = 0, bool showEars = false, bool showLefEars = false, int padding = 2, RenderMode renderMode = RenderMode.Full, params Tuple<int, string>[] itemEntries)
            => GetCharacter(id, animation, frame, showEars, showLefEars, padding, renderMode, itemEntries.Select(c => new Tuple<int, string, int?>(c.Item1, c.Item2, null)).ToArray());

        public Image<Rgba32> GetCharacter(int id, string animation = null, int frame = 0, bool showEars = false, bool showLefEars = false, int padding = 2, RenderMode renderMode = RenderMode.Full, params Tuple<int, string, int?>[] itemEntries)
        {
            Stopwatch watch = Stopwatch.StartNew();
            CharacterAvatar avatar = new CharacterAvatar(wz);
            avatar.Equips = itemEntries.Select(c => new EquipSelection(){ ItemId = c.Item1, AnimationName = c.Item2 }).ToArray();

            avatar.SkinId = id;
            avatar.AnimationName = animation;
            if (string.IsNullOrEmpty(avatar.AnimationName)) avatar.AnimationName = "stand1";
            avatar.FrameNumber = frame;
            avatar.ElfEars = showEars;
            avatar.LefEars = showLefEars;
            avatar.Padding = padding;
            avatar.Mode = renderMode;

            var result = avatar.Render();
            return result;
        }

        public string[] GetActions(params int[] itemEntries)
        {
            List<string> itemEntriesStr = itemEntries.Concat(new int[]{ 1060002, 1040002 }).Where(c => c >= 30000).Select(c => c.ToString("D8")).ToList();
            IEnumerable<WZProperty> itemNodes = wz.Resolve("Character").Children.Values
                .Where(c => c.Type != PropertyType.Image)
                .SelectMany(c => c.Children.Values)
                .Where(c => itemEntriesStr.Contains(c.Name));

            string[] firstItemAnimations = itemNodes.Where(c => c.Name.Equals("01040002")).First().Children.Keys.ToArray();
            return itemNodes.Skip(1)
                .SelectMany(c => c.Children.Keys.Where(firstItemAnimations.Contains))
                .Distinct()
                .ToArray();
        }

        public string[] GetActions(params Equip[] eqps)
        {
            CharacterSkin skin = GetSkin(2000);
            eqps = eqps.Where(c => c.FrameBooks.ContainsKey("stand1") || c.FrameBooks.ContainsKey("stand2")).ToArray();

            return skin.Animations.Where(c => c.Value.AnimationName.Equals(c.Key, StringComparison.CurrentCultureIgnoreCase)).Select(c => c.Key).Where(c => eqps.All(e => e.FrameBooks.ContainsKey(c))).ToArray();
        }

        public byte[] GetSpriteSheet(HttpRequest request, int id, bool showEars = false, bool showLefEars = false, int padding = 2, RenderMode renderMode = RenderMode.Full, params int[] itemEntries)
        {
            Stopwatch watch = Stopwatch.StartNew();
            Equip face = itemEntries.Where(c => c >= 20000 && c <= 25000).Select(c => (Equip)itemFactory.search(c)).FirstOrDefault();

            CharacterSkin skin = GetSkin(id);
            CharacterAvatar avatar = new CharacterAvatar(wz);

            avatar.Equips = itemEntries.Select(c => new EquipSelection(){ ItemId = c }).ToArray();

            avatar.SkinId = id;
            avatar.AnimationName = "stand1";
            avatar.FrameNumber = 0;
            avatar.ElfEars = showEars;
            avatar.LefEars = showLefEars;
            avatar.Padding = padding;
            avatar.Preload();

            string[] actions = GetActions(itemEntries);

            List<Func<Tuple<string, Image<Rgba32>>>> allImages = new List<Func<Tuple<string, Image<Rgba32>>>>();

            foreach (string emotion in face?.FrameBooks?.Keys?.ToArray() ?? new[] { "default" })
            {
                int emotionFrames = face?.FrameBooks[emotion]?.frames?.Count() ?? 1;
                foreach(int emotionFrame in Enumerable.Range(0, emotionFrames)) {
                    foreach (string animation in actions)
                    {
                        if (!skin.Animations.ContainsKey(animation)) continue;

                        foreach(int frame in Enumerable.Range(0, skin.Animations[animation].Frames.Length))
                        {
                            if (watch.ElapsedMilliseconds > 120000) return null;
                            allImages.Add(() => {
                                Tuple<int, string, int?>[] items = itemEntries
                                    .Select(c => new Tuple<int, string, int?>(c, (c == face?.id) ? emotion : null, (c == face?.id) ? (int?)emotionFrame : null))
                                    .ToArray();
                                string path = $"{emotion}/{emotionFrame}/{animation}_{frame}.png";
                                try {
                                    CharacterAvatar frameAvatar = new CharacterAvatar(avatar);
                                    frameAvatar.AnimationName = animation;
                                    frameAvatar.FrameNumber = frame;
                                    // We can modify the equips array, but if we change the actual contents other than the face there could be problems.
                                    frameAvatar.equipped = frameAvatar.equipped.Select(o => new Tuple<WZProperty, EquipSelection>(o.Item1,
                                        new EquipSelection(){
                                            ItemId = o.Item2.ItemId,
                                            AnimationName = o.Item2?.ItemId == face?.id ? emotion : o.Item2.AnimationName,
                                            EquipFrame = o.Item2?.ItemId == face?.id ? emotionFrame : o.Item2.EquipFrame
                                        })
                                    ).ToArray();
                                    var res = new Tuple<string, Image<Rgba32>>(
                                        path,
                                        frameAvatar.Render()
                                    );
                                    return res;
                                } catch (Exception) {
                                    return null;
                                }
                            });
                        }
                    }
                }
            }

            using (MemoryStream mem = new MemoryStream())
            {
                using (ZipArchive archive = new ZipArchive(mem, ZipArchiveMode.Create, true))
                {
                    ConcurrentBag<Tuple<string, byte[]>> bag = new ConcurrentBag<Tuple<string, byte[]>>();
                    Parallel.ForEach(allImages, (a) => {
                        var b = a();
                        if (b == null) return;
                        bag.Add(new Tuple<string, byte[]>(b.Item1, b.Item2.ImageToByte(request)));
                    });

                    foreach(Tuple<string, byte[]> frameData in bag) {
                        ZipArchiveEntry entry = archive.CreateEntry(frameData.Item1, CompressionLevel.Optimal);
                        using (Stream entryData = entry.Open()) {
                            entryData.Write(frameData.Item2, 0, frameData.Item2.Length);
                            entryData.Flush();
                        }
                    }
                }

                return mem.ToArray();
            }
        }

        public override ICharacterFactory GetWithWZ(Region region, string version)
            => new CharacterFactory(_factory, itemFactory.GetWithWZ(region, version), _zmapFactory.GetWithWZ(region, version), _logger, region, version);
    }
}
