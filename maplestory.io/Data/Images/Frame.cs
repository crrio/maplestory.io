using PKG1;
using SixLabors.ImageSharp;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace maplestory.io.Data.Images
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

        public static Frame Parse(WZProperty value)
        {
            if (value == null) return null;
            Frame animationFrame = new Frame();

            animationFrame.LazyImage = new Lazy<Image<Rgba32>>(() => value.ResolveForOrNull<Image<Rgba32>>());
            animationFrame.delay = value.ResolveFor<int>("delay") ?? value.Resolve().ResolveFor<int>("delay");
            animationFrame.Origin = value.ResolveFor<Point>("origin") ?? value.Resolve()?.ResolveFor<Point>("origin") ?? new Point(animationFrame.Image?.Width / 2 ?? 0, animationFrame.Image?.Height / 2 ?? 0);
            animationFrame.Position = value.ResolveForOrNull<string>("z") ?? value.ResolveForOrNull<string>("../z") ?? value.Resolve().ResolveForOrNull<string>("z") ?? value.Resolve().ResolveForOrNull<string>("../z");
            animationFrame.MapOffset = (value.Resolve("map") ?? value.Resolve().Resolve("map"))?.Children
                .Where(c => c.Type == PropertyType.Vector2)
                .ToDictionary(b => b.NameWithoutExtension, b => b.ResolveFor<Point>() ?? Point.Empty);

            int relativePosition = value.ResolveFor<int>("pos") ?? value.ResolveFor<int>("../pos") ?? 0;
            Point bodyRelativeMove = value.ResolveFor<Point>("../info/bodyRelMove") ?? value.ResolveFor<Point>("../../info/bodyRelMove") ?? value.ResolveFor<Point>("../../../info/bodyRelMove") ?? new Point();

            if (animationFrame.MapOffset == null) {
                if (relativePosition == 1)
                    animationFrame.MapOffset = new Dictionary<string, Point>() { { "navel", new Point(0 + (bodyRelativeMove.X), 31 + (bodyRelativeMove.Y) * 4) } }; // *shrug*
                else
                    animationFrame.MapOffset = new Dictionary<string, Point>() { { "zero", new Point(0 + (bodyRelativeMove.X), 0 + (bodyRelativeMove.Y)) } };
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