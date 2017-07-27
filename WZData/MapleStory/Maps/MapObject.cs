using System;
using System.Collections.Generic;
using ImageSharp;
using System.Text;
using PKG1;
using System.Linq;
using System.Numerics;
using WZData.MapleStory.Images;

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

        public static MapObject Parse(WZProperty data)
        {
            MapObject result = new MapObject();
            result.pathToImage = string.Join("/", (new []{
                data.ResolveForOrNull<string>("oS"),
                data.ResolveForOrNull<string>("l0"),
                data.ResolveForOrNull<string>("l1"),
                data.ResolveForOrNull<string>("l2"),
            }).Where(c => c != null));
            result.Position = new Vector3(
                data.ResolveFor<float>("x") ?? 0,
                data.ResolveFor<float>("y") ?? 0,
                data.ResolveFor<float>("z") ?? 0
            );
            result.Rotation = data.ResolveFor<float>("r");
            WZProperty objCanvas = data.ResolveOutlink($"Map/Obj/{result.pathToImage}");
            result.Canvas = Frame.Parse(objCanvas.Children.Values.FirstOrDefault() ?? objCanvas);
            return result;
        }
    }
}
