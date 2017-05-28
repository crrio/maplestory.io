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

        public bool IsTown;
        public bool IsSwim;
        public string MapMark;
        public bool IsMiniMap;

        public static Map Parse(WZFile mapWz, MapName name)
        {
            Map result = new Map();

            result.Id = name.Id;
            result.Name = name.Name;
            result.StreetName = name.StreetName;

            WZObject mapEntry = mapWz.ResolvePath($"Map/Map{result.Id.ToString("D8")[0]}/{result.Id.ToString("D8")}.img");

            result.ProcessInfo(mapEntry.ResolvePath("info"));
            result.portals = mapEntry.ResolvePath("portal").Select(Portal.Parse);

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
            IsMiniMap = info.HasChild("miniMap");
            MapMark = info.HasChild("mapMark") ? info["mapMark"].ValueOrDefault<string>(null) : null;
        }
    }
}
