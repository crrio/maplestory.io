﻿using reWZ.WZProperties;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using MoreLinq;
using System.Text;
using System.Linq;

namespace WZData.MapleStory.Characters
{
    public class CharacterSkin
    {
        public Dictionary<string, BodyAnimation> Animations;
        public int Id;

        public CharacterSkin(int Id, WZObject bodyContainer, WZObject headContainer)
        {
            this.Id = Id;
            Animations = Enumerable.Concat(headContainer, bodyContainer)
                .AsParallel()
                // Filter out any non-frame containing animations
                .Where(c => !c.Any(b => !int.TryParse(b.Name, out int test)))
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
                            HasFace = b.Where(d => d.HasFace.HasValue).Any(d => d.HasFace ?? false),
                            Parts = b.Select(d => d.Parts).SelectMany(d => d).DistinctBy(d => d.Key).ToDictionary(d => d.Key, d => d.Value)
                        })
                        .ToArray()
                })
                .ToDictionary(c => c.AnimationName);
        }

        public static CharacterSkin[] Parse(WZObject characterWz)
            => characterWz
                .Where(c => int.TryParse(c.Name.Replace(".img", ""), out int blah) && blah < 10000)
                .Select(c => int.Parse(c.Name.Replace(".img", "")))
                .Select(c => new CharacterSkin(c, characterWz[$"{c.ToString("D8")}.img"], characterWz[$"{(c + 10000).ToString("D8")}.img"]))
                .ToArray();
    }

    public class BodyAnimation
    {
        public string AnimationName;
        public Body[] Frames;

        static ConcurrentDictionary<string, BodyAnimation> cache = new ConcurrentDictionary<string, BodyAnimation>();

        public static BodyAnimation Parse(WZObject animation)
        {
            if (cache.ContainsKey(animation.Path)) return cache[animation.Path];

            BodyAnimation result = new BodyAnimation();

            result.AnimationName = animation.Name;
            result.Frames = animation.Select(Body.Parse).ToArray();

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

        internal static Body Parse(WZObject frame, int frameNumber)
        {
            Body result = new Body();

            result.FrameNumber = frameNumber;
            result.HasFace = frame.HasChild("face") ? (bool?)(frame["face"].ValueOrDefault<int>(0) == 1) : null;
            result.Delay = frame.HasChild("delay") ? (int?)frame["delay"].ValueOrDefault<int>(0) : null;
            result.Parts = ResolveParts(frame);

            return result;
        }

        private static Dictionary<string, BodyPart> ResolveParts(WZObject frame)
        {
            if (frame.HasChild("action"))
            {
                string action = frame["action"].ValueOrDefault<string>("");
                int frameNumber = frame.HasChild("frame") ? frame["frame"].ValueOrDefault<int>(0) : 0;
                return ResolveParts(frame.ResolvePath($"../../{action}/{frameNumber}"));
            }

            Dictionary<string, BodyPart> parts = frame.Where(c => c.Name != "delay" && c.Name != "face" && c.Name != "hideName" && c.Name != "move")
                .Select(BodyPart.Parse)
                .Where(a => a != null)
                .ToDictionary(a => a.Name);
            while (!cache.TryAdd(frame.Path, parts) && !cache.ContainsKey(frame.Path)) ;
            return parts;
        }
    }

    public class BodyPart : IFrame
    {
        public string Name;
        public Bitmap Image { get; set; }
        public Point? Origin { get; set; }
        public string Position { get; set; }
        public Dictionary<string, Point> MapOffset { get; set; }

        static ConcurrentDictionary<string, BodyPart> cache = new ConcurrentDictionary<string, BodyPart>();

        internal static BodyPart Parse(WZObject part)
        {
            if (part is WZCanvasProperty)
            {
                if (cache.ContainsKey(part.Path)) return cache[part.Path];

                BodyPart result = new BodyPart();

                result.Name = part.Name;
                result.Image = ((WZCanvasProperty)part).ValueOrDefault<Bitmap>(null);
                result.Origin = part.HasChild("origin") ? ((WZPointProperty)part["origin"]).Value : new Point(0, 0);
                result.Position = part.HasChild("z") ? part["z"].ValueOrDefault<string>("") : null;
                result.MapOffset = part.HasChild("map") ? part["map"].Where(c => c is WZPointProperty).Select(c => new Tuple<string, Point>(c.Name, ((WZPointProperty)c).Value)).ToDictionary(b => b.Item1, b => b.Item2) : null;

                while (!cache.TryAdd(part.Path, result) && !cache.ContainsKey(part.Path)) ;

                return result;
            }
            else if (part is WZUOLProperty)
                try
                {
                    return Parse(((WZUOLProperty)part).Resolve());
                }catch(Exception ex) { return null; }
            return null;
        }
    }
}