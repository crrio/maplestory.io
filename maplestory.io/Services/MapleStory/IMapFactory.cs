using SixLabors.ImageSharp;
using System.Collections.Generic;
using WZData.MapleStory.Maps;

namespace maplestory.io.Services.MapleStory
{
    public interface IMapFactory : INeedWZ<IMapFactory>
    {
        IEnumerable<MapName> GetMapNames();
        MapName GetMapName(int id);
        Map GetMap(int id, bool followLinks = true);
        MapMark GetMapMark(string markName);
        Image<Rgba32> Render(int id, bool showLife, bool showPortals, bool showBackgrounds);
    }
}
