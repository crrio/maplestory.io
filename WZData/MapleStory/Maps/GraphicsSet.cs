using System;
using System.Collections.Generic;
using ImageSharp;
using System.Text;
using PKG1;
using System.Linq;
using System.Numerics;

namespace WZData.MapleStory.Maps
{
    public class GraphicsSet
    {
        public string TileSet;
        public IEnumerable<MapObject> Objects;
        public IEnumerable<MapTile> Tiles;

        public static GraphicsSet Parse(WZProperty data)
        {
            GraphicsSet result = new GraphicsSet();
            result.TileSet = data.ResolveForOrNull<string>("info/tS");
            result.Objects = data.Resolve("obj").Children.Values.Select(c => MapObject.Parse(c));
            result.Tiles = data.Resolve("tile").Children.Values.Select(c => MapTile.Parse(c, result.TileSet));

            return result;
        }

        public Image<Rgba32> Render() {

        }
    }
}
