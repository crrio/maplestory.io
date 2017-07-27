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
    public class MapTile : IPositionedFrameContainer
    {
        public string pathToImage;
        public bool FrontMost;
        public bool Flip { get; set; }
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
        public static MapTile Parse(WZProperty data, string tileSet)
        {
            MapTile result = new MapTile();
            result.pathToImage = string.Join("/", new []{
                tileSet,
                data.ResolveForOrNull<string>("u"),
                data.ResolveForOrNull<string>("no")
            });
            WZProperty tileCanvas = data.ResolveOutlink($"Map/Tile/{result.pathToImage}");
            if (tileCanvas == null) return null;
            result.Canvas = Frame.Parse(tileCanvas.Children.Values.FirstOrDefault(c => c.Type == PropertyType.Canvas) ?? tileCanvas);
            result.FrontMost = data.ResolveFor<bool>("front") ?? false;
            result.Flip = data.ResolveFor<bool>("f") ?? false;
            if (result.Flip && result.Canvas != null && result.Canvas.Image != null)
                result.Canvas.Image = new Image<Rgba32>(result.Canvas.Image).Flip(FlipType.Horizontal);
            result.Position = new Vector3(
                data.ResolveFor<int>("x") ?? 0,
                data.ResolveFor<int>("y") ?? 0,
                result.FrontMost ? 100000000 : (tileCanvas.ResolveFor<int>("z") ?? 1)
            );
            return result;
        }
    }
}