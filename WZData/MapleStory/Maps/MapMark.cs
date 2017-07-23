using ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PKG1;

namespace WZData.MapleStory.Maps
{
    public class MapMark
    {
        public string Name;
        public Image<Rgba32> Mark;

        public static IEnumerable<MapMark> Parse(PackageCollection mapWz)
            => mapWz.Resolve("Map/MapHelper.img/mark").Children.Values.Select(mark => new MapMark() { Mark = mark.ResolveForOrNull<Image<Rgba32>>(), Name = mark.Name });

        public static MapMark Parse(WZProperty mark)
            => new MapMark() { Mark = mark.ResolveForOrNull<Image<Rgba32>>(), Name = mark.Name };
    }
}
