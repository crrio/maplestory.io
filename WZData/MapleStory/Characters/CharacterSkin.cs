﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ImageSharp;
using MoreLinq;
using System.Text;
using System.Linq;
using System.Numerics;
using PKG1;

namespace WZData.MapleStory.Characters
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

            Animations = Enumerable.Concat(headContainer.Children.Values, bodyContainer.Children.Values)
                // Filter out any non-frame containing animations
                .Where(c => !c.Children.Any(b => !int.TryParse(b.Key, out int test)))
                // Map the animations to partial BodyAnimations
                .Select(animation => BodyAnimation.Parse(animation))
                .GroupBy(c => c.AnimationName)
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
                .ToDictionary(c => c.AnimationName);
        }

        public static IEnumerable<CharacterSkin> Parse(WZProperty characterWz)
            => characterWz.Children.Values
                .Where(c => int.TryParse(c.Name.Replace(".img", ""), out int blah) && blah < 10000)
                .Select(c => int.Parse(c.Name.Replace(".img", "")))
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

            result.AnimationName = animation.Name;
            result.Frames = animation.Children.Values.Select(Body.Parse).ToArray();

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
            if (frame.Children.ContainsKey("action"))
            {
                string action = frame.ResolveForOrNull<string>("action");
                int frameNumber = frame.ResolveFor<int>("frame") ?? 0;
                return ResolveParts(frame.Resolve($"../../{action}/{frameNumber}"));
            }

            Dictionary<string, BodyPart> parts = frame.Children.Where(c => !blacklistPartElements.Contains(c.Key))
                .Select(c => BodyPart.Parse(c.Value))
                .Where(a => a != null)
                .ToDictionary(a => a.Name);
            while (!cache.TryAdd(frame.Path, parts) && !cache.ContainsKey(frame.Path)) ;
            return parts;
        }
    }

    public class BodyPart : IFrame
    {
        public string Name;
        public Image<Rgba32> Image { get; set; }
        public Point? Center { get; set; }
        public string Position { get; set; }
        public Dictionary<string, Point> MapOffset { get; set; }
        internal static BodyPart Parse(WZProperty part)
        {
            if (part.Type == PropertyType.Canvas)
            {
                BodyPart result = new BodyPart();

                result.Name = part.Name;
                result.Image = part.ResolveForOrNull<Image<Rgba32>>();
                result.Center = part.ResolveFor<Point>("origin");
                result.Position = part.ResolveForOrNull<string>("z") ?? part.ResolveForOrNull<string>("../z");
                result.MapOffset = part.Resolve("map")?.Children
                    .Where(c => c.Value.Type == PropertyType.Vector2)
                    .ToDictionary(b => b.Key, b => b.Value.ResolveFor<Point>() ?? Point.Empty);

                return result;
            }
            else if (part.Type == PropertyType.UOL){
                return Parse(part.Resolve());
            }
            return null;
        }
    }
}
