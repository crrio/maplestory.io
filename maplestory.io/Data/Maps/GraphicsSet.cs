using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using System.Text;
using PKG1;
using System.Linq;
using System.Numerics;
using maplestory.io.Data.Images;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace maplestory.io.Data.Maps
{
    public class GraphicsSet
    {
        public string TileSet;
        public IEnumerable<MapObject> Objects;
        public IEnumerable<MapTile> Tiles;
        public int Index;

        public static GraphicsSet Parse(WZProperty data, int index)
        {
            GraphicsSet result = new GraphicsSet();
            result.Index = index;
            result.TileSet = data.ResolveForOrNull<string>("info/tS");
            result.Objects = data.Resolve("obj")?.Children.Select(c => MapObject.Parse(c)).Where(c => c != null);
            result.Tiles = data.Resolve("tile")?.Children.Select(c => MapTile.Parse(c, result.TileSet)).Where(c => c != null);

            return result;
        }
    }
}
