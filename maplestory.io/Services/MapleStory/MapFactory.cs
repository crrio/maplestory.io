﻿using MoreLinq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WZData.MapleStory.Maps;
using WZData.MapleStory.Quests;
using PKG1;

namespace maplestory.io.Services.MapleStory
{
    public class MapFactory : NeedWZ<IMapFactory>, IMapFactory
    {
        public MapFactory(IWZFactory factory) : base(factory) { }
        public MapFactory(IWZFactory factory, Region region, string version) : base(factory, region, version) { }

        public Map GetMap(int id) {
            MapName name = GetMapName(id);
            Map map = Map.Parse(name, wz);
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
                .First();
            MapName name = MapName.Parse(mapName);
            return name;
        }
        public override IMapFactory GetWithWZ(Region region, string version)
            => new MapFactory(_factory, region, version);
    }
}
