using maplestory.io.Data.Images;
using PKG1;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;
using System.Linq;

namespace maplestory.io.Data.Maps
{
    public class WorldMap
    {
        public string WorldMapName;
        public string ParentWorld;
        public Frame[] BaseImage;
        public WorldMapLink[] Links;
        public DirectMaps[] Maps;

        public static WorldMap Parse(WZProperty worldMapNode)
        {
            if (worldMapNode == null) return null;

            WorldMap result = new WorldMap();
            result.BaseImage = worldMapNode.Resolve("BaseImg").Children.Select(c => Frame.Parse(c)).ToArray();
            result.WorldMapName = worldMapNode.ResolveForOrNull<string>("info/WorldMap");
            result.ParentWorld = worldMapNode.ResolveForOrNull<string>("info/parentMap");
            result.Links = worldMapNode.Resolve("MapLink").Children.Select(c => WorldMapLink.Parse(c)).Where(c => c != null).ToArray();
            result.Maps = worldMapNode.Resolve("MapList").Children.Select(c => DirectMaps.Parse(c)).Where(c => c != null).ToArray();

            return result;
        }

        public class WorldMapLink
        {
            public string ToolTip;
            public string LinksTo;
            public Frame LinkImage;

            public static WorldMapLink Parse(WZProperty prop)
            {
                if (prop == null) return null;

                WorldMapLink result = new WorldMapLink();
                result.ToolTip = prop.ResolveForOrNull<string>("toolTip");
                result.LinkImage = Frame.Parse(prop.Resolve("link/linkImg"));
                result.LinksTo = prop.ResolveForOrNull<string>("link/linkMap");

                return result;
            }
        }

        public class DirectMaps
        {
            public bool? NoTooltip;
            public string Title;
            public string Description;
            public Point? Spot;
            public int? Type;
            public int[] MapNumbers;

            public static DirectMaps Parse(WZProperty prop)
            {
                if (prop == null) return null;

                DirectMaps result = new DirectMaps();
                result.Spot = prop.ResolveFor<Point>("spot");
                result.Type = prop.ResolveFor<int>("type");
                result.Title = prop.ResolveForOrNull<string>("desc");
                result.Description = prop.ResolveForOrNull<string>("title");
                result.NoTooltip = prop.ResolveFor<bool>("noToolTip");
                result.MapNumbers = prop.Resolve("mapNo").Children.Select(c => c.ResolveFor<int>()).Where(c => c.HasValue).Select(c => c.Value).ToArray();

                return result;
            }
        }
    }
}
