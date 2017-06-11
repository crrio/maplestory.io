using ImageSharp;
using reWZ;
using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WZData.ItemMetaInfo
{
    public class IconInfo
    {
        public Image<Rgba32> IconRaw;
        public Image<Rgba32> Icon;

        public static IconInfo Parse(WZDirectory source, WZObject info)
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

            if (!(info.HasChild("icon") || info.HasChild("iconRaw")))
                return null;

            results.Icon = Resolve(source, info, "icon");
            results.IconRaw = Resolve(source, info, "iconRaw");

            return results;
        }

        static Image<Rgba32> Resolve(WZDirectory source, WZObject info, string propertyName)
        {
            Image<Rgba32> Icon = null;

            if (!info.HasChild(propertyName))
                return Icon;

            if (info[propertyName].HasChild("source"))
            {
                string path = info[propertyName]["source"].ValueOrDefault<string>("");
                path = path.Substring(path.IndexOf("/"));
                Icon = source.ResolvePath(path).ImageOrDefault();
            }
            else if (info[propertyName].HasChild("_inlink"))
            {
                string path = info[propertyName]["_inlink"].ValueOrDefault<string>("");
                if (path.StartsWith("info"))
                    Icon = info.ResolvePath(path.Substring(path.IndexOf("/"))).ImageOrDefault();
                else
                    try
                    {
                        Icon = source.ResolvePath(Path.Combine(info.Path, "../../", path)).ImageOrDefault();
                    } catch (Exception)
                    {
                        Icon = source.ResolvePath(Path.Combine(info.Path, "../", path)).ImageOrDefault();
                    }
            }
            else if (info[propertyName].HasChild("_outlink"))
            {
                string path = info[propertyName]["_outlink"].ValueOrDefault<string>("");
                path = path.Substring(path.IndexOf("/"));
                Icon = source.ResolvePath(path).ImageOrDefault();
            }
            else
            {
                Icon = info[propertyName].ImageOrDefault();
                if (Icon == null)
                {
                    string iconPath = info[propertyName].ValueOrDefault<string>(null);
                    if (iconPath != null)
                    {
                        try
                        {
                            Icon = info.ResolvePath(iconPath).ImageOrDefault();
                        } catch (Exception ex)
                        {
                            // Possible trying to jump to another wz image, look for 4 digit number and add .img extension
                            string[] parts = iconPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                            string imgPath = parts.Where(p => p.Length == 4).Where(p =>
                            {
                                int test = 0;
                                return int.TryParse(p, out test);
                            }).FirstOrDefault();

                            // No possible path from here
                            if (imgPath == null) return null;

                            iconPath = iconPath.Replace($"/{imgPath}/", $"/{imgPath}.img/");
                            Icon = info.ResolvePath(iconPath).ImageOrDefault();
                        }
                    }
                }
            }

            return Icon;
        }
    }
}
