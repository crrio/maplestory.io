using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using PKG1;

namespace WZData.MapleStory.Maps
{
    public class MapName
    {
        public string Name, StreetName;
        public int Id;

        public static MapName Parse(WZProperty mapEntry)
        {
            return new MapName()
            {
                Id = int.Parse(mapEntry.Name),
                Name = mapEntry.ResolveForOrNull<string>("mapName"),
                StreetName = mapEntry.ResolveForOrNull<string>("streetName")
            };
        }

        public static IEnumerable<MapName> GetMapNames(WZProperty stringWz)
            => stringWz.Resolve("Map").Children.Values.SelectMany(c => c.Children.Values).Select(c => MapName.Parse(c));
    }
}
