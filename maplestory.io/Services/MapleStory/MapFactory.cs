using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WZData.MapleStory.Maps;

namespace maplestory.io.Services.MapleStory
{
    public class MapFactory : IMapFactory
    {
        private readonly Dictionary<int, MapName> mapNames;
        private readonly Dictionary<int, Func<Map>> mapLookup;
        private readonly Dictionary<string, MapMark> markLookup;

        public MapFactory(IWZFactory factory)
        {
            mapNames = MapName.GetMapNames(factory.GetWZFile(WZ.String)).DistinctBy(c => c.Id).ToDictionary(c => c.Id);
            markLookup = MapMark.Parse(factory.GetWZFile(WZ.Map)).ToDictionary(mark => mark.Name);
            mapLookup = new Dictionary<int, Func<Map>>();
            foreach (MapName mapName in mapNames.Values)
                mapLookup.Add(mapName.Id, CreateLookup(factory, mapName));
        }

        Func<Map> CreateLookup(IWZFactory factory, MapName mapName) => () => Map.Parse(factory.GetWZFile(WZ.Map), mapName);

        public Map GetMap(int id) => mapLookup[id]();
        public MapMark GetMapMark(string markName) => markLookup[markName];
        public IEnumerable<MapName> GetMapNames() => mapNames.Values;
        public MapName GetMapName(int id) => mapNames[id];
    }
}
