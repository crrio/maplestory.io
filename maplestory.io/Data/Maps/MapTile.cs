using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using System.Text;
using PKG1;
using System.Linq;
using System.Numerics;
using maplestory.io.Data.Images;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace maplestory.io.Data.Maps
{
    public class MapTile : IPositionedFrameContainer
    {
        public string pathToImage;
        public bool FrontMost;
        public bool Flip { get; set; }
        public Frame Canvas { get; set; }
        public Vector3 Position { get; set; }
        public RectangleF Bounds {
            get => Canvas == null ? new RectangleF(Position.X, Position.Y, 1, 1) : new RectangleF(Position.X - Canvas.OriginOrZero.X, Position.Y - Canvas.OriginOrZero.Y, Canvas?.Image.Width ?? 1, Canvas?.Image.Height ?? 1);
        }
        public static MapTile Parse(WZProperty data, string tileSet, int frame)
        {
            MapTile result = new MapTile();
            result.pathToImage = string.Join("/", new []{
                tileSet,
                data.ResolveForOrNull<string>("u"),
                data.ResolveForOrNull<string>("no")
            });
            WZProperty tileCanvas = data.ResolveOutlink($"Map/Tile/{result.pathToImage}");
            if (tileCanvas == null) return null;
            int frameCount = tileCanvas.Resolve().Children.Select(c => int.TryParse(c.NameWithoutExtension, out int frameNum) ? (int?)frameNum : null).Where(c => c.HasValue).Select(c => c.Value).DefaultIfEmpty(0).Max();
            result.Canvas = Frame.Parse(tileCanvas.Resolve((frame % (frameCount + 1)).ToString()) ?? tileCanvas);
            result.FrontMost = data.ResolveFor<bool>("front") ?? false;
            result.Flip = data.ResolveFor<bool>("f") ?? false;
            if (result.Flip && result.Canvas != null && result.Canvas.Image != null)
                result.Canvas.Image = result.Canvas.Image.Clone(c => c.Flip(FlipType.Horizontal));
            result.Position = new Vector3(
                data.ResolveFor<int>("x") ?? 0,
                data.ResolveFor<int>("y") ?? 0,
                result.FrontMost ? 100000000 : (tileCanvas.ResolveFor<int>("z") ?? 1)
            );
            return result;
        }
    }
}
