using reWZ;
using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WZData.MapleStory.Maps
{
    public class Map : MapName
    {
        public string BackgroundMusic;
        public bool IsReturnMap;
        public int ReturnMap;
        public IEnumerable<Portal> portals;
        public IEnumerable<MapLife> NPCs;
        public IEnumerable<MapLife> Mobs;

        public bool IsTown;
        public bool IsSwim;
        public string MapMark;
        public MiniMap MiniMap;

        public static Map Parse(WZFile mapWz, MapName name)
        {
            Map result = new Map();

            result.Id = name.Id;
            result.Name = name.Name;
            result.StreetName = name.StreetName;

            WZObject mapEntry = mapWz.ResolvePath($"Map/Map{result.Id.ToString("D8")[0]}/{result.Id.ToString("D8")}.img");

            result.ProcessInfo(mapEntry.ResolvePath("info"));
            result.portals = mapEntry.HasChild("portal") ? mapEntry.ResolvePath("portal").Select(Portal.Parse) : new Portal[0];
            result.MiniMap = mapEntry.HasChild("miniMap") ? result.MiniMap = MiniMap.Parse(mapEntry.ResolvePath("miniMap")) : null;

            IEnumerable<MapLife> life = mapEntry.HasChild("life") ? mapEntry["life"].Select(c => MapLife.Parse(c)) : null;
            result.NPCs = life?.Where(c => c.Type == LifeType.NPC);
            result.Mobs = life?.Where(c => c.Type == LifeType.Monster);

            return result;
        }

        private void ProcessInfo(WZObject info)
        {
            BackgroundMusic = info.HasChild("bgm") ? info["bgm"].ValueOrDefault<string>(null) : null;
            ReturnMap = info.HasChild("returnMap") ? info["returnMap"].ValueOrDefault<int>(0) : -1;
            IsReturnMap = ReturnMap == Id;
            IsReturnMap = ReturnMap == 999999999;
            IsTown = info.HasChild("town") && info["town"].ValueOrDefault<int>(0) == 1;
            IsSwim = info.HasChild("swim") && info["swim"].ValueOrDefault<int>(0) == 1;
            MapMark = info.HasChild("mapMark") ? info["mapMark"].ValueOrDefault<string>(null) : null;
        }
    }
}
