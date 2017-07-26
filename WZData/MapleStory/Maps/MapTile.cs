using System;
using System.Collections.Generic;
using ImageSharp;
using System.Text;
using PKG1;
using System.Linq;
using System.Numerics;

namespace WZData.MapleStory.Maps
{
    public class MapTile
    {
        public string pathToImage;
        public Point Position;
        public Frame Canvas;
        public static MapTile Parse(WZProperty data, string tileSet)
        {
            MapTile result = new MapTile();
            result.pathToImage = string.Join("/", new []{
                tileSet,
                data.ResolveForOrNull<string>("u"),
                data.ResolveForOrNull<string>("no")
            });
            WZProperty tileCanvas = data.Resolve($"Map/Tile/{result.pathToImage}");
            result.Canvas = Frame.Parse(tileCanvas.Children.Values.FirstOrDefault() ?? tileCanvas);
            result.Position = new Point(
                data.ResolveFor<int>("x") ?? 0,
                data.ResolveFor<int>("y") ?? 0
            );
            return result;
        }
    }
}
