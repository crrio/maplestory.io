using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace WZData.MapleStory.Maps
{
    public class MiniMap
    {
        public int centerX, centerY, height, width, magnification;
        public Bitmap canvas;

        public static MiniMap Parse(WZObject data)
        {
            MiniMap result = new MiniMap();
            result.canvas = data.HasChild("canvas") ? data["canvas"].ValueOrDefault<Bitmap>(null) : null;
            result.centerX = data.HasChild("centerX") ? data["centerX"].ValueOrDefault<int>(-1) : -1;
            result.centerY = data.HasChild("centerY") ? data["centerY"].ValueOrDefault<int>(-1) : -1;
            result.height = data.HasChild("height") ? data["height"].ValueOrDefault<int>(-1) : -1;
            result.width = data.HasChild("width") ? data["width"].ValueOrDefault<int>(-1) : -1;
            result.magnification = data.HasChild("mag") ? data["mag"].ValueOrDefault<int>(-1) : -1;
            return result;
        }
    }
}
