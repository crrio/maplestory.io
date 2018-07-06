using PKG1;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;
using System;
using System.Linq;

namespace maplestory.io.Data.Items
{
    public class IconInfo
    {
        readonly static string[] mustContainOne = new []{ "icon", "iconRaw" };
        public Image<Rgba32> IconRaw;
        public Image<Rgba32> Icon;
        public Point? IconOrigin;
        public Point? IconRawOrigin;

        public static IconInfo Parse(WZProperty info)
        {
            IconInfo results = new IconInfo();

            string infoPath = info.Path;
            string itemId = infoPath.Substring(infoPath.Length - 13, 8);
            int id = -1;
            if (int.TryParse(itemId, out id)) {
                string iconName = null;
                //Rank D Nebulite
                if (3060000 <= id && id < 3061000) iconName = "nebulite-D";
                //Rank C Nebulite
                if (3061000 <= id && id < 3062000) iconName = "nebulite-C";
                //Rank B Nebulite
                if (3062000 <= id && id < 3063000) iconName = "nebulite-B";
                //Rank A Nebulite
                if (3063000 <= id && id < 3064000) iconName = "nebulite-A";

                if (iconName != null)
                {
                    Image<Rgba32> icon = Image.Load($"assets/{iconName}.png");
                    results.Icon = icon;
                    results.IconRaw = icon;

                    return results;
                }
            }

            if (!info.Children.Any(c => mustContainOne.Contains(c.NameWithoutExtension)))
                return null;

            results.Icon = info.ResolveForOrNull<Image<Rgba32>>("icon");
            results.IconRaw = info.ResolveForOrNull<Image<Rgba32>>("iconRaw");
            results.IconOrigin = info.ResolveFor<Point>("icon/origin");
            results.IconRawOrigin = info.ResolveFor<Point>("iconRaw/origin");

            return results;
        }
    }
}
