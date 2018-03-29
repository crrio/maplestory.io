using SixLabors.ImageSharp;
using System.Collections.Generic;
using maplestory.io.Data.Maps;

namespace maplestory.io.Services.Interfaces.MapleStory
{
    public interface IMapFactory
    {
        IEnumerable<MapName> GetMapNames();
        MapName GetMapName(int id);
        Map GetMap(int id, bool followLinks = true);
        MapMark GetMapMark(string markName);
        Image<Rgba32> Render(int id, bool showLife, bool showPortals, bool showBackgrounds);
    }
}
