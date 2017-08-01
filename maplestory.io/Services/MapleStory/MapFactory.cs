using MoreLinq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WZData.MapleStory.Maps;
using WZData.MapleStory.Quests;
using PKG1;
using ImageSharp;

namespace maplestory.io.Services.MapleStory
{
    public class MapFactory : NeedWZ<IMapFactory>, IMapFactory
    {
        public MapFactory(IWZFactory factory) : base(factory) { }
        public MapFactory(IWZFactory factory, Region region, string version) : base(factory, region, version) { }

        public Map GetMap(int id, bool followLinks = true) {
            MapName name = GetMapName(id);
            Map map = Map.Parse(id, name, wz);
            Map link = map;
            List<int> triedMaps = new List<int>();
            while (link != null && link.LinksTo != null && followLinks) {
                if (triedMaps.Contains(link.LinksTo ?? id)) break;
                link = GetMap(link.LinksTo ?? id, false);
                triedMaps.Add(link.Id);
            }
            if (link != map) map.ExtendFrom(link);
            return map;
        }
        public MapMark GetMapMark(string markName) => MapMark.Parse(_factory.GetWZ(region, version).Resolve($"Map/MapHelper.img/mark/{markName}"));
        public IEnumerable<MapName> GetMapNames() {
            return wz.Resolve("String/Map").Children.Values
                .SelectMany(c => c.Children)
                .Select(c => MapName.Parse(c.Value));
        }
        public MapName GetMapName(int id) {
            WZProperty mapName = wz.Resolve("String/Map").Children.Values
                .SelectMany(c => c.Children)
                .Where(c => c.Key == id.ToString())
                .Select(c => c.Value)
                .FirstOrDefault();
            MapName name = MapName.Parse(mapName);
            return name;
        }
        public override IMapFactory GetWithWZ(Region region, string version)
            => new MapFactory(_factory, region, version);

        public Image<Rgba32> Render(int id, bool showLife, bool showPortals)
            => GetMap(id)?.Render(showLife, showPortals);
    }
}
