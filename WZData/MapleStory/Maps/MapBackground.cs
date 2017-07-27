using System;
using System.Collections.Generic;
using ImageSharp;
using System.Text;
using PKG1;
using System.Linq;
using System.Numerics;
using WZData.MapleStory.Images;
using ImageSharp.Processing;
using SixLabors.Primitives;

namespace WZData.MapleStory.Maps
{
    public class MapBackground : IPositionedFrameContainer
    {
        public string BackgroundSet;
        public string pathToImage;
        public bool Flip { get; set; }
        public float Alpha;
        public BackgroundType Type;
        private bool Front;
        public Frame Canvas { get; set; }
        public Vector3 Position { get; set; }
        public RectangleF Bounds {
            get {
                Point canvasOrigin = Canvas.Origin ?? new Point(Canvas.Image.Width / 2, Canvas.Image.Height / 2);
                return new RectangleF(
                    Position.X - canvasOrigin.X,
                    Position.Y - canvasOrigin.Y,
                    Canvas.Image.Width,
                    Canvas.Image.Height
                );
            }
        }
        public static MapBackground Parse(WZProperty data)
        {
            MapBackground result = new MapBackground();
            result.BackgroundSet = data.ResolveForOrNull<string>("bS");
            result.pathToImage = string.Join("/", new []{
                result.BackgroundSet, // backgroundSet,
                "back",
                data.ResolveForOrNull<string>("no")
            });
            result.Front = data.ResolveFor<bool>("front") ?? false;
            result.Alpha = (data.ResolveFor<int>("a") ?? 255) / 255;
            result.Flip = data.ResolveFor<bool>("f") ?? false;
            WZProperty tileCanvas = data.ResolveOutlink($"Map/Back/{result.pathToImage}");
            if (tileCanvas != null) // Could be null as we're not supporting ani backgrounds
                result.Canvas = Frame.Parse(tileCanvas?.Children.Values.FirstOrDefault(c => c.Type == PropertyType.Canvas) ?? tileCanvas);
            if (result.Flip && result.Canvas != null && result.Canvas.Image != null)
                result.Canvas.Image = new Image<Rgba32>(result.Canvas.Image).Flip(FlipType.Horizontal);
            result.Type = (BackgroundType)(data.ResolveFor<int>("type") ?? 0);
            result.Position = new Vector3(
                data.ResolveFor<int>("x") ?? 0,
                data.ResolveFor<int>("y") ?? 0,
                result.Front ? 100000000 : int.Parse(data.Name)
            );
            return result;
        }
    }

    public enum BackgroundType {
        Single = 0,
        TiledHorizontal = 1,
        TiledVertical = 2,
        TiledBoth = 3,
        ScrollingTiledHorizontal = 4,
        ScrollingTiledVertical = 5,
        ScrollingHorizontalTiledBoth = 6,
        ScrollingVerticalTiledBoth = 7
    }
}
