using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WZData.MapleStory.Maps;

namespace maplestory.io.Services.MapleStory
{
    public interface IMapFactory
    {
        IEnumerable<MapName> GetMapNames();
        MapName GetMapName(int id);
        Map GetMap(int id);
        MapMark GetMapMark(string markName);
    }
}
