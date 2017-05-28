using reWZ;
using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace WZData.MapleStory.Maps
{
    public class MapName
    {
        public string Name, StreetName;
        public int Id;
        
        public static MapName Parse(WZObject mapEntry)
        {
            return new MapName()
            {
                Id = int.Parse(mapEntry.Name),
                Name = mapEntry.HasChild("mapName") ? mapEntry["mapName"].ValueOrDefault<string>(null) : null,
                StreetName = mapEntry.HasChild("streetName") ? mapEntry["streetName"].ValueOrDefault<string>(null) : null
            };
        }

        public static IEnumerable<MapName> GetMapNames(WZFile stringWz)
            => stringWz.ResolvePath("Map.img").SelectMany(c => c).Select(c => MapName.Parse(c));
    }
}
