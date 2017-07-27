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
using ImageSharp.PixelFormats;

namespace WZData.MapleStory.Maps
{
    public class MapObject : IPositionedFrameContainer
    {
        public string pathToImage;
        public Frame Canvas { get; set; }
        public Vector3 Position { get; set; }

        public RectangleF Bounds  {
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

        public float? Rotation;
        public bool FrontMost;
        public int[] Quests;
        public string Tags;
        public bool Flip { get; set; }
        public static MapObject Parse(WZProperty data)
        {
            MapObject result = new MapObject();
            result.pathToImage = string.Join("/", (new []{
                data.ResolveForOrNull<string>("oS"),
                data.ResolveForOrNull<string>("l0"),
                data.ResolveForOrNull<string>("l1"),
                data.ResolveForOrNull<string>("l2"),
            }).Where(c => c != null));
            result.FrontMost = data.ResolveFor<bool>("front") ?? false;
            result.Position = new Vector3(
                data.ResolveFor<float>("x") ?? 0,
                data.ResolveFor<float>("y") ?? 0,
                result.FrontMost ? 100000000 : Math.Max(data.ResolveFor<float>("z") ?? 0, data.ResolveFor<float>("zM") ?? 0)
            );
            result.Quests = data.Resolve("quest")?.Children?.Keys
                .Where(c => int.TryParse(c, out int blah))
                .Select(c => int.Parse(c))
                .ToArray();
            result.Tags = data.ResolveForOrNull<string>("tags");
            result.Rotation = data.ResolveFor<float>("r");
            WZProperty objCanvas = data.ResolveOutlink($"Map/Obj/{result.pathToImage}");
            if (objCanvas == null) return null;
            result.Canvas = Frame.Parse(objCanvas.Children.Values.FirstOrDefault(c => c.Type == PropertyType.Canvas) ?? objCanvas);
            result.Flip = data.ResolveFor<bool>("f") ?? false;
            if (result.Flip)
                result.Canvas.Image = result.Canvas.Image.Flip(FlipType.Vertical);

            return result;
        }
    }
}
