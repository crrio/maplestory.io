using maplestory.io.Data.Images;
using MoreLinq;
using PKG1;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace maplestory.io.Data.Characters
{
    public class CharacterSkin
    {
        public Dictionary<string, BodyAnimation> Animations;
        public int Id;
        string bodyISlot, headISlot;
        string bodyVSlot, headVSlot;

        public CharacterSkin(int Id, WZProperty bodyContainer, WZProperty headContainer)
        {
            this.Id = Id;

            headISlot = headContainer.ResolveForOrNull<string>("info/islot") ?? "Hd";
            headVSlot = headContainer.ResolveForOrNull<string>("info/vslot") ?? "Hd";
            bodyISlot = bodyContainer.ResolveForOrNull<string>("info/islot") ?? "Bd";
            bodyVSlot = bodyContainer.ResolveForOrNull<string>("info/vslot") ?? "Bd";

            Animations = Enumerable.Concat(headContainer.Children, bodyContainer.Children)
                // Filter out any non-frame containing animations
                .Where(c => !c.Children.Any(b => !int.TryParse(b.NameWithoutExtension, out int test)))
                // Map the animations to partial BodyAnimations
                .Select(animation => BodyAnimation.Parse(animation))
                .GroupBy(c => c.AnimationName)
                .AsParallel()
                .Select(c => new BodyAnimation()
                {
                    AnimationName = c.First().AnimationName,
                    Frames = c.Select(b => b.Frames)
                        .SelectMany(b => b)
                        .GroupBy(b => b.FrameNumber)
                        .Select(b => new Body()
                        {
                            Delay = (int)(b.Where(d => d.Delay.HasValue).Select(d => d.Delay.Value).DefaultIfEmpty(0).Average()),
                            FrameNumber = b.First().FrameNumber,
                            HasFace = b.Where(d => d.HasFace.HasValue).Any(d => d.HasFace ?? true),
                            Parts = b.Select(d => d.Parts).SelectMany(d => d).DistinctBy(d => d.Key).ToDictionary(d => d.Key, d => d.Value)
                        })
                        .ToArray()
                })
                .AsParallel()
                .ToDictionary(c => c.AnimationName);
        }

        public static CharacterSkin Parse(WZProperty characterWz, int id)
            => new CharacterSkin(id, characterWz.Resolve($"{id.ToString("D8")}"), characterWz.Resolve($"{(id + 10000).ToString("D8")}"));
        public static IEnumerable<CharacterSkin> Parse(WZProperty characterWz)
            => characterWz.Children
                .Where(c => int.TryParse(c.NameWithoutExtension.Replace(".img", ""), out int blah) && blah < 10000)
                .Select(c => int.Parse(c.NameWithoutExtension.Replace(".img", "")))
                .Select(c => new CharacterSkin(c, characterWz.Resolve($"{c.ToString("D8")}"), characterWz.Resolve($"{(c + 10000).ToString("D8")}")));
    }

    public class BodyAnimation
    {
        public string AnimationName;
        public Body[] Frames;

        static ConcurrentDictionary<string, BodyAnimation> cache = new ConcurrentDictionary<string, BodyAnimation>();

        public static BodyAnimation Parse(WZProperty animation)
        {
            if (cache.ContainsKey(animation.Path)) return cache[animation.Path];

            BodyAnimation result = new BodyAnimation();

            result.AnimationName = animation.NameWithoutExtension;
            result.Frames = animation.Children.Select(Body.Parse).ToArray();

            while (!cache.TryAdd(animation.Path, result) && !cache.ContainsKey(animation.Path)) ;

            return result;
        }
    }

    public class Body
    {
        public bool? HasFace;
        public int? Delay;
        public Dictionary<string, BodyPart> Parts;
        public int FrameNumber;

        static ConcurrentDictionary<string, Dictionary<string, BodyPart>> cache = new ConcurrentDictionary<string, Dictionary<string, BodyPart>>();

        internal static Body Parse(WZProperty frame, int frameNumber)
        {
            Body result = new Body();

            result.FrameNumber = frameNumber;
            result.HasFace = frame.ResolveFor<bool>("face");
            result.Delay = frame.ResolveFor<int>("delay");
            result.Parts = ResolveParts(frame);

            return result;
        }

        static readonly string[] blacklistPartElements = new []{ "delay", "face", "hideName", "move" };
        private static Dictionary<string, BodyPart> ResolveParts(WZProperty frame)
        {
            if (frame.Children.Any(c => c.NameWithoutExtension.Equals("action")))
            {
                string action = frame.ResolveForOrNull<string>("action");
                int frameNumber = frame.ResolveFor<int>("frame") ?? 0;
                return ResolveParts(frame.Resolve($"../../{action}/{frameNumber}"));
            }

            Dictionary<string, BodyPart> parts = frame.Children.Where(c => !blacklistPartElements.Contains(c.NameWithoutExtension))
                .Select(c => BodyPart.Parse(c))
                .Where(a => a != null)
                .ToDictionary(a => a.Name);
            while (!cache.TryAdd(frame.Path, parts) && !cache.ContainsKey(frame.Path)) ;
            return parts;
        }
    }

    public class BodyPart : IFrame
    {
        public string Name;
        Image<Rgba32> cached;
        Func<Image<Rgba32>> load;
        public Image<Rgba32> Image { get{
            if (cached == null) load();
            return cached;
        } set => cached = value; }
        public Point? Origin { get; set; }
        public string Position { get; set; }
        public Dictionary<string, Point> MapOffset { get; set; }
        internal static BodyPart Parse(WZProperty part)
        {
            if (part.Type == PropertyType.Canvas)
            {
                BodyPart result = new BodyPart();

                result.Name = part.NameWithoutExtension;
                result.load = () => result.Image = part.ResolveForOrNull<Image<Rgba32>>();
                // result.
                result.Origin = part.ResolveFor<Point>("origin");
                result.Position = part.ResolveForOrNull<string>("z") ?? part.ResolveForOrNull<string>("../z");
                result.MapOffset = part.Resolve("map")?.Children
                    .Where(c => c.Type == PropertyType.Vector2)
                    .ToDictionary(b => b.NameWithoutExtension, b => b.ResolveFor<Point>() ?? Point.Empty);

                return result;
            }
            else if (part.Type == PropertyType.UOL){
                return Parse(part.Resolve());
            }
            return null;
        }
    }
}
