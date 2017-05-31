using MoreLinq;
using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WZData.MapleStory.Maps;
using WZData.MapleStory.Quests;

namespace maplestory.io.Services.MapleStory
{
    public class MapFactory : IMapFactory
    {
        private readonly Dictionary<int, MapName> mapNames;
        private readonly Dictionary<int, Func<Map>> mapLookup;
        private readonly Dictionary<string, MapMark> markLookup;

        public MapFactory(IWZFactory factory)
        {
            WZObject mapWz = factory.GetWZFile(WZ.Map).MainDirectory;

            IEnumerable<MapName> tempMapNames = MapName.GetMapNames(factory.GetWZFile(WZ.String))
                .DistinctBy(c => c.Id)
                .OrderBy(c => c.Id);

            markLookup = MapMark.Parse(factory.GetWZFile(WZ.Map))
                .ToDictionary(mark => mark.Name);

            mapNames = new Dictionary<int, MapName>();
            mapLookup = new Dictionary<int, Func<Map>>();
            foreach (MapName mapName in tempMapNames)
            {
                if (!mapWz["Map"][$"Map{mapName.Id.ToString("D9")[0]}"].HasChild($"{mapName.Id.ToString("D9")}.img"))
                    continue;

                mapNames.Add(mapName.Id, mapName);
                mapLookup.Add(mapName.Id, CreateLookup(factory, mapName));
            }
        }

        Func<Map> CreateLookup(IWZFactory factory, MapName mapName) => () => Map.Parse(factory.GetWZFile(WZ.Map), mapName);

        public Map GetMap(int id) => mapLookup[id]();
        public MapMark GetMapMark(string markName) => markLookup[markName];
        public IEnumerable<MapName> GetMapNames() => mapNames.Values;
        public MapName GetMapName(int id) => mapNames[id];
    }
}
