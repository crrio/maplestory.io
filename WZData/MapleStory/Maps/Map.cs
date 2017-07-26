using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImageSharp;
using Newtonsoft.Json;
using PKG1;

namespace WZData.MapleStory.Maps
{
    public class Map : MapName
    {
        public string BackgroundMusic;
        public bool? IsReturnMap;
        public int? ReturnMap;
        public IEnumerable<Portal> portals;
        public IEnumerable<MapLife> Npcs;
        public IEnumerable<MapLife> Mobs;
        public double? MobRate;
        public bool? IsTown;
        public bool? IsSwim;
        public string MapMark;
        public MiniMap MiniMap;
        [JsonIgnore]
        public IEnumerable<GraphicsSet> Graphics;

        public static Map Parse(MapName name, PackageCollection collection)
        {
            Map result = new Map();

            result.Id = name.Id;
            result.Name = name.Name;
            result.StreetName = name.StreetName;

            WZProperty mapEntry = collection.Resolve($"Map/Map/Map{result.Id.ToString("D8")[0]}/{result.Id.ToString("D8")}.img");
            WZProperty mapInfo = mapEntry.Resolve("info");

            result.BackgroundMusic = mapInfo.ResolveForOrNull<string>("bgm");
            result.ReturnMap = mapInfo.ResolveFor<int>("returnMap");
//            result.IsReturnMap = result.ReturnMap == result.Id;
            result.IsReturnMap = result.ReturnMap == 999999999;
            result.IsTown = mapInfo.ResolveFor<bool>("town");
            result.IsSwim = mapInfo.ResolveFor<bool>("swim");
            result.MobRate = mapInfo.ResolveFor<double>("mobRate");
            result.MapMark = mapInfo.ResolveForOrNull<string>("mapMark");

            result.portals = mapEntry.Resolve("portal").Children.Values.Select(Portal.Parse);
            result.MiniMap = result.MiniMap = MiniMap.Parse(mapEntry.Resolve("miniMap"));

            IEnumerable<MapLife> life = mapEntry.Resolve("life").Children.Values.Select(MapLife.Parse);
            result.Npcs = life?.Where(c => c.Type == LifeType.NPC);
            result.Mobs = life?.Where(c => c.Type == LifeType.Monster);
            result.Graphics = mapEntry.Children.Keys
                .Where(c => int.TryParse(c, out int blah))
                .Select(c => GraphicsSet.Parse(mapEntry.Children[c]));

            return result;
        }

        public Image<Rgba32> Render()
        {

        }
    }
}
