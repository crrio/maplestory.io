using PKG1;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using System.Linq;

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
