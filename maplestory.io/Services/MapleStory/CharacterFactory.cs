using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PKG1;
using SixLabors.ImageSharp;
using SixLabors.Primitives;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using WZData.MapleStory;
using WZData.MapleStory.Characters;
using WZData.MapleStory.Images;
using WZData.MapleStory.Items;

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
            : base(factory, region, version) {
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
            => GetCharacter(id, animation, frame, showEars, showLefEars, padding, null, 1, false, renderMode, new Tuple<int, string, float?>[0]);

        public Image<Rgba32> GetBaseWithHair(int id, string animation = null, int frame = 0, bool showEars = false, bool showLefEars = false, int padding = 2, int faceId = 20305, int hairId = 37831, RenderMode renderMode = RenderMode.Full)
            => GetCharacter(id, animation, frame, showEars, showLefEars, padding, null, 1, false, renderMode, new Tuple<int, string, float?>(faceId, null, null), new Tuple<int, string, float?>(hairId, null, null));

        CharacterAvatar getAvatar(int id, string animation = null, int frame = 0, bool showEars = false, bool showLefEars = false, int padding = 2, string name = null, float zoom = 1, bool flipX = false, params Tuple<int, string, float?>[] itemEntries)
        {
            CharacterAvatar avatar = new CharacterAvatar(wz);
            avatar.Equips = itemEntries.Select(c => new EquipSelection() { ItemId = c.Item1, AnimationName = c.Item2, Hue = c.Item3 }).ToArray();

            avatar.SkinId = id;
            avatar.AnimationName = animation;
            if (string.IsNullOrEmpty(avatar.AnimationName)) avatar.AnimationName = "stand1";
            avatar.FrameNumber = frame;
            avatar.ElfEars = showEars;
            avatar.LefEars = showLefEars;
            avatar.Padding = padding;
            avatar.Name = name;
            avatar.Zoom = zoom;
            avatar.FlipX = flipX;

            return avatar;
        }

        public Tuple<Image<Rgba32>, Dictionary<string, Point>, Dictionary<string, int>> GetDetailedCharacter(int id, string animation = null, int frame = 0, bool showEars = false, bool showLefEars = false, int padding = 2, string name = null, float zoom = 1, bool flipX = false, RenderMode renderMode = RenderMode.Full,  params Tuple<int, string, float?>[] itemEntries)
        {
            CharacterAvatar avatar = getAvatar(id, animation, frame, showEars, showLefEars, padding, name, zoom, flipX, itemEntries);
            avatar.Mode = renderMode;
            return avatar.RenderWithDetails();
        }

        public IEnumerable<Tuple<Frame, Point, float?>> GetJsonCharacter(int id, string animation = null, int frame = 0, bool showEars = false, bool showLefEars = false, int padding = 2, string name = null, float zoom = 1, bool flipX = false, params Tuple<int, string, float?>[] itemEntries)
        {
            CharacterAvatar avatar = getAvatar(id, animation, frame, showEars, showLefEars, padding, name, zoom, flipX, itemEntries);
            return avatar.GetFrameParts().Select(c => new Tuple<Frame, Point, float?>(c.Item1, c.Item2, c.Item3));
        }

        public Image<Rgba32> GetCharacter(int id, string animation = null, int frame = 0, bool showEars = false, bool showLefEars = false, int padding = 2, string name = null, float zoom = 1, bool flipX = false, RenderMode renderMode = RenderMode.Full, params Tuple<int, string, float?>[] itemEntries)
        {
            CharacterAvatar avatar = getAvatar(id, animation, frame, showEars, showLefEars, padding, name, zoom, flipX, itemEntries);
            return avatar.Render();
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

            if (eqps.Any(c => c.id >= 1902000 && c.id <= 1993000)) eqps = eqps.Where(c => c.id >= 1902000 && c.id <= 1993000).ToArray();

            return skin.Animations.Where(c => c.Value.AnimationName.Equals(c.Key, StringComparison.CurrentCultureIgnoreCase)).Select(c => c.Key).Where(c => eqps.All(e => e.FrameBooks.ContainsKey(c))).ToArray();
        }

        public byte[] GetSpriteSheet(HttpRequest request, int id, bool showEars = false, bool showLefEars = false, int padding = 2, RenderMode renderMode = RenderMode.Full, SpriteSheetFormat format = SpriteSheetFormat.Plain, params Tuple<int, float?>[] itemEntries)
        {
            Stopwatch watch = Stopwatch.StartNew();
            Equip face = itemEntries.Where(c => c.Item1 >= 20000 && c.Item1 <= 29999).Select(c => (Equip)itemFactory.search(c.Item1)).FirstOrDefault();

            CharacterSkin skin = GetSkin(id);
            CharacterAvatar avatar = new CharacterAvatar(wz);

            avatar.Equips = itemEntries.Select(c => new EquipSelection(){ ItemId = c.Item1, Hue = c.Item2 }).ToArray();

            avatar.SkinId = id;
            avatar.AnimationName = "stand1";
            avatar.FrameNumber = 0;
            avatar.ElfEars = showEars;
            avatar.LefEars = showLefEars;
            avatar.Padding = padding;
            avatar.Mode = renderMode;
            avatar.Preload();

            string fileExtension = "png";
            if (format == SpriteSheetFormat.PDNZip) fileExtension = "zip";

            string[] actions = GetActions(itemEntries.Select(c => c.Item1).ToArray());

            List<Func<Tuple<string, byte[]>>> allImages = new List<Func<Tuple<string, byte[]>>>();

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
                                    .Select(c => new Tuple<int, string, int?>(c.Item1, (c.Item1 == face?.id) ? emotion : null, (c.Item1 == face?.id) ? (int?)emotionFrame : null))
                                    .ToArray();
                                string path = $"{emotion}/{emotionFrame}/{animation}_{frame}.{fileExtension}";
                                try {
                                    CharacterAvatar frameAvatar = new CharacterAvatar(avatar);
                                    frameAvatar.AnimationName = animation;
                                    frameAvatar.FrameNumber = frame;
                                    // We can modify the equips array, but if we change the actual contents other than the face there could be problems.
                                    frameAvatar.equipped = frameAvatar.equipped.Select(o => new Tuple<WZProperty, EquipSelection>(o.Item1,
                                        new EquipSelection(){
                                            ItemId = o.Item2.ItemId,
                                            AnimationName = o.Item2?.ItemId == face?.id ? emotion : o.Item2.AnimationName,
                                            EquipFrame = o.Item2?.ItemId == face?.id ? emotionFrame : o.Item2.EquipFrame,
                                            Hue = o.Item2.Hue
                                        })
                                    ).ToArray();
                                    var res = new Tuple<string, byte[]>(
                                        path,
                                        frameAvatar.Render(format, img => img.ImageToByte(request))
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
                        bag.Add(new Tuple<string, byte[]>(b.Item1, b.Item2));
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
