using System;
using ImageSharp;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using PKG1;
using SixLabors.Primitives;

namespace WZData
{
    public class Frame : IFrame
    {
        Lazy<Image<Rgba32>> LazyImage;
        public Image<Rgba32> Image { get => LazyImage.Value; set { LazyImage = new Lazy<Image<Rgba32>>(() => value); } }
        public int? delay;
        public Point? Origin { get; set; }
        public Point OriginOrZero { get => Origin ?? Point.Empty; }
        public Dictionary<string, Point> MapOffset { get; set; }
        public string Position { get; set; }

        internal static Frame Parse(WZProperty value)
        {
            if (value == null) return null;
            Frame animationFrame = new Frame();

            animationFrame.LazyImage = new Lazy<Image<Rgba32>>(() => value.ResolveForOrNull<Image<Rgba32>>());
            animationFrame.delay = value.ResolveFor<int>("delay") ?? value.Resolve().ResolveFor<int>("delay");
            animationFrame.Origin = value.ResolveFor<Point>("origin") ?? value.Resolve()?.ResolveFor<Point>("origin") ?? new Point(animationFrame.Image?.Width / 2 ?? 0, animationFrame.Image?.Height / 2 ?? 0);
            animationFrame.Position = value.ResolveForOrNull<string>("z") ?? value.ResolveForOrNull<string>("../z") ?? value.Resolve().ResolveForOrNull<string>("z") ?? value.Resolve().ResolveForOrNull<string>("../z");
            animationFrame.MapOffset = (value.Resolve("map") ?? value.Resolve().Resolve("map"))?.Children
                .Where(c => c.Value.Type == PropertyType.Vector2)
                .ToDictionary(b => b.Key, b => b.Value.ResolveFor<Point>() ?? Point.Empty);

            if (animationFrame.MapOffset == null && !(value.ResolveFor<bool>("../pos") ?? false)) {
                animationFrame.MapOffset = new Dictionary<string, Point>() { { "zero", new Point(0, 0) } };
            }

            return animationFrame;
        }

        public override string ToString()
         => $"IFrame ({Position})";
    }

    public interface IFrame
    {
        Image<Rgba32> Image { get; }
        Point? Origin { get; }
        string Position { get; }
        Dictionary<string, Point> MapOffset { get; }
    }
}