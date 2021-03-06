﻿using maplestory.io.Data.Maps;
using maplestory.io.Services.Interfaces.MapleStory;
using PKG1;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using System.Linq;

namespace maplestory.io.Services.Implementations.MapleStory
{
    public class MapFactory : NeedWZ, IMapFactory
    {
        public Map GetMap(int id, bool followLinks = true) {
            MapName name = GetMapName(id);
            Map map = Map.Parse(id, name, WZ);
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
        public string[] GetWorldMaps() => WZ.Resolve("Map/WorldMap").Children.Select(c => c.NameWithoutExtension).ToArray();
        public WorldMap GetWorldMap(string id)
        {
            WZProperty worldMapNode = WZ.Resolve($"Map/WorldMap/{id}");
            return WorldMap.Parse(worldMapNode);
        }
        public MapMark GetMapMark(string markName) => MapMark.Parse(WZ.Resolve($"Map/MapHelper.img/mark/{markName}"));
        public IEnumerable<MapName> GetMapNames(string searchFor = null, int startPosition = 0, int? count = null) {
            if (!string.IsNullOrEmpty(searchFor)) searchFor.ToLower();
            return WZ.Resolve("String/Map").Children
                .SelectMany(c => c.Children)
                .Where(c =>
                {
                    int mapId = 0;
                    if (!int.TryParse(c.NameWithoutExtension, out mapId)) return false;
                    string eightDigitId = mapId.ToString("D8");
                    string nineDigitId = mapId.ToString("D9");
                    WZProperty eightDigits = WZ.Resolve($"Map/Map/Map{eightDigitId[0]}");
                    WZProperty nineDigits = WZ.Resolve($"Map/Map/Map{nineDigitId[0]}");
                    return (eightDigits?.Children?.Any(m => m.NameWithoutExtension.Equals(eightDigitId) || m.NameWithoutExtension.Equals(nineDigitId)) ?? false) || (nineDigits?.Children?.Any(m => m.NameWithoutExtension.Equals(eightDigitId) || m.NameWithoutExtension.Equals(nineDigitId)) ?? false);
                })
                .Where(c => c != null)
                .Select(c => MapName.Parse(c))
                .Where(c => string.IsNullOrEmpty(searchFor) || (!string.IsNullOrEmpty(c.Name) && c.Name.ToLower().Contains(searchFor)) || (!string.IsNullOrEmpty(c.StreetName) && c.StreetName.ToLower().Contains(searchFor)))
                .Skip(startPosition)
                .Take(count ?? int.MaxValue);
        }
        public MapName GetMapName(int id) {
            WZProperty mapName = WZ.Resolve("String/Map").Children
                .SelectMany(c => c.Children)
                .Where(c => c.NameWithoutExtension == id.ToString())
                .FirstOrDefault();
            MapName name = MapName.Parse(mapName);
            return name;
        }
        public Image<Rgba32> Render(int id, int frame, bool showLife, bool showPortals, bool showBackgrounds)
        {
            Map entry = GetMap(id);
            MapRender renderer = new MapRender(entry, entry.mapEntry);
            return renderer.Render(frame, showLife, showPortals, showBackgrounds);
        }

        public Image<Rgba32> RenderLayer(int mapId, int layer, int frame, bool filterTrash, int? minX = null, int? minY = null)
        {
            Map entry = GetMap(mapId);
            MapRender renderer = new MapRender(entry, entry.mapEntry);
            return renderer.RenderLayer(frame, layer, filterTrash, minX, minY);
        }

        public Image<Rgba32> RenderLayer(int mapId, int layer, int frame)
        {
            Map entry = GetMap(mapId);
            MapRender renderer = new MapRender(entry, entry.mapEntry);
            return renderer.RenderLayer(frame, layer);
        }

        public MapRenderPlan GetRenderPlan(int mapId) {
            Map entry = GetMap(mapId);
            return new MapRenderPlan(entry, entry.mapEntry);
        }
    }
}
