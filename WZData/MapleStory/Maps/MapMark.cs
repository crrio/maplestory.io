using ImageSharp;
using reWZ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WZData.MapleStory.Maps
{
    public class MapMark
    {
        public string Name;
        public Image<Rgba32> Mark;

        public static IEnumerable<MapMark> Parse(WZFile mapWz)
            => mapWz.ResolvePath("MapHelper.img/mark").Select(mark => new MapMark() { Mark = mark.ImageOrDefault(), Name = mark.Name });
    }
}
