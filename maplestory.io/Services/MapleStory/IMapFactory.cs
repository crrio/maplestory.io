using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImageSharp;
using PKG1;
using WZData.MapleStory.Maps;

namespace maplestory.io.Services.MapleStory
{
    public interface IMapFactory : INeedWZ<IMapFactory>
    {
        IEnumerable<MapName> GetMapNames();
        MapName GetMapName(int id);
        Map GetMap(int id, bool followLinks = true);
        MapMark GetMapMark(string markName);
        Image<Rgba32> Render(int id, bool showLife, bool showPortals);
    }
}
