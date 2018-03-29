using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PKG1;

namespace maplestory.io.Data.Maps
{
    public class MapMark
    {
        public string Name;
        public Image<Rgba32> Mark;

        public static IEnumerable<MapMark> Parse(PackageCollection mapWz)
            => mapWz.Resolve("Map/MapHelper.img/mark").Children.Select(mark => new MapMark() { Mark = mark.ResolveForOrNull<Image<Rgba32>>(), Name = mark.NameWithoutExtension });

        public static MapMark Parse(WZProperty mark)
            => new MapMark() { Mark = mark.ResolveForOrNull<Image<Rgba32>>(), Name = mark.NameWithoutExtension };
    }
}
