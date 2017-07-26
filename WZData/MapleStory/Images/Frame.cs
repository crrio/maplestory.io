using System;
using ImageSharp;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using PKG1;

namespace WZData
{
    public class Frame : IFrame
    {
        public Image<Rgba32> Image { get; set; }
        public int? delay;
        public Point? Center { get; set; }
        public Dictionary<string, Point> MapOffset { get; set; }
        public string Position { get; set; }

        internal static Frame Parse(WZProperty value)
        {
            Frame animationFrame = new Frame();

            animationFrame.Image = value.ResolveForOrNull<Image<Rgba32>>();
            animationFrame.delay = value.ResolveFor<int>("delay") ?? value.Resolve().ResolveFor<int>("delay");
            animationFrame.Center = value.ResolveFor<Point>("origin") ?? value.Resolve().ResolveFor<Point>("origin") ?? new Point(animationFrame.Image.Width / 2, animationFrame.Image.Height / 2);
            animationFrame.Position = value.ResolveForOrNull<string>("z") ?? value.ResolveForOrNull<string>("../z") ?? value.Resolve().ResolveForOrNull<string>("z") ?? value.Resolve().ResolveForOrNull<string>("../z");
            animationFrame.MapOffset = (value.Resolve("map") ?? value.Resolve().Resolve("map"))?.Children
                .Where(c => c.Value.Type == PropertyType.Vector2)
                .ToDictionary(b => b.Key, b => b.Value.ResolveFor<Point>() ?? Point.Empty);

            if (animationFrame.MapOffset == null && value.ResolveFor<int>("../pos") == 1) {
                animationFrame.MapOffset = new Dictionary<string, Point>() {
                    { "neck", new Point(0,0) }
                };
            }

            return animationFrame;
        }

        public override string ToString()
         => $"IFrame ({Position})";
    }

    public interface IFrame
    {
        Image<Rgba32> Image { get; }
        Point? Center { get; }
        string Position { get; }
        Dictionary<string, Point> MapOffset { get; }
    }
}