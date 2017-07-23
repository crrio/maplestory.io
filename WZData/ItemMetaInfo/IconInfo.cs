using ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PKG1;

namespace WZData.ItemMetaInfo
{
    public class IconInfo
    {
        readonly static string[] mustContainOne = new []{ "icon", "iconRaw" };
        public Image<Rgba32> IconRaw;
        public Image<Rgba32> Icon;

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

            if (!info.Children.Keys.Any(c => mustContainOne.Contains(c)))
                return null;

            results.Icon = info.ResolveForOrNull<Image<Rgba32>>("icon");
            results.IconRaw = info.ResolveForOrNull<Image<Rgba32>>("iconRaw");

            return results;
        }
    }
}
