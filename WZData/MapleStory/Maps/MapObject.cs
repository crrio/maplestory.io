using System;
using System.Collections.Generic;
using ImageSharp;
using System.Text;
using PKG1;
using System.Linq;
using System.Numerics;

namespace WZData.MapleStory.Maps
{
    public class MapObject
    {
        public string pathToImage;
        public Frame Canvas;
        public Vector3 Position;
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
            WZProperty objCanvas = data.Resolve($"Map/Obj/{result.pathToImage}");
            result.Canvas = Frame.Parse(objCanvas.Children.Values.FirstOrDefault() ?? objCanvas);
            return result;
        }
    }
}
