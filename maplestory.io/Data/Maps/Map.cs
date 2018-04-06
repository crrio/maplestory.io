using MoreLinq;
using Newtonsoft.Json;
using PKG1;
using SixLabors.ImageSharp;
using SixLabors.Primitives;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using maplestory.io.Data.Images;

namespace maplestory.io.Data.Maps
{
    public class Map : MapName
    {
        public string BackgroundMusic;
        public bool? IsReturnMap;
        public int? ReturnMap;
        public Portal[] portals = new Portal[0];
        public IEnumerable<MapLife> Npcs = new MapLife[0];
        public IEnumerable<MapLife> Mobs = new MapLife[0];
        public double? MobRate;
        public bool? IsTown;
        public bool? IsSwim;
        public string MapMark;
        public MiniMap MiniMap;
        public int? LinksTo;
        [JsonIgnore]
        internal IEnumerable<MapLife> Life;
        public Dictionary<int, Foothold> Footholds;
        public int? MinimumStarForce;
        public int? MinimumArcaneForce;
        public int? MinimumLevel;
        public RectangleF VRBounds;
        public MapName ReturnMapName;
        [JsonIgnore]
        internal WZProperty mapEntry;

        public static Map Parse(int id, MapName name, PackageCollection collection, List<int> attemptedIds = null)
        {
            if (attemptedIds == null) attemptedIds = new List<int>();

            Stopwatch watch = Stopwatch.StartNew();
            Map result = new Map();
            result.Id = id;

            if (name != null) {
                result.Id = name.Id;
                result.Name = name.Name;
                result.StreetName = name.StreetName;
            }

            WZProperty mapEntry = collection.Resolve($"Map/Map/Map{id.ToString("D8")[0]}/{id.ToString("D8")}.img");
            mapEntry = mapEntry ?? collection.Resolve($"Map/Map/Map{id.ToString("D9")[0]}/{id.ToString("D9")}.img");
            mapEntry = mapEntry ?? collection.Resolve($"Map/Map/Map{result.Id.ToString("D8")[0]}/{result.Id.ToString("D8")}.img");
            mapEntry = mapEntry ?? collection.Resolve($"Map/Map/Map{result.Id.ToString("D9")[0]}/{result.Id.ToString("D9")}.img");
            result.mapEntry = mapEntry;

            int? linkedTo = mapEntry.ResolveFor<int>("info/link");
            if (linkedTo.HasValue && !attemptedIds.Contains(linkedTo.Value))
            {
                watch.Stop();
                attemptedIds.Add(id);
                attemptedIds.Add(result.Id);
                return Parse(linkedTo.Value, name, collection, attemptedIds);
            }

            Parse(mapEntry, result);

            watch.Stop();
            Package.Logging($"Map Parse took {watch.ElapsedMilliseconds}");

            return result;
        }

        public static Map Parse(WZProperty mapEntry, Map result)
        {
            if (mapEntry == null) return null;
            WZProperty mapInfo = mapEntry.Resolve("info");

            ParseInfo(result, mapEntry, mapInfo);
            ParseFootholds(result, mapEntry);
            ParseLife(result, mapEntry);

            return result;
        }

        private static void ParseLife(Map result, WZProperty mapEntry)
        {
            Stopwatch watch = Stopwatch.StartNew();
            ConcurrentDictionary<int, Tuple<string, Frame>> lifeTemplateCache = new ConcurrentDictionary<int, Tuple<string, Frame>>();
            result.Life = mapEntry.Resolve("life")?.Children
                .GroupBy(c => c.ResolveFor<int>("id"))
                .Select(grouping => grouping.Select(c => MapLife.Parse(c, result.Footholds, lifeTemplateCache)).ToArray())
                .SelectMany(c => c).ToArray();
            result.Npcs = result.Life?.Where(c => c.Type == LifeType.NPC).ToArray();
            result.Mobs = result.Life?.Where(c => c.Type == LifeType.Monster).ToArray();
            watch.Stop();
            Package.Logging($"Map ParseLife took {watch.ElapsedMilliseconds}");
        }

        private static void ParseFootholds(Map result, WZProperty mapEntry)
        {
            Stopwatch watch = Stopwatch.StartNew();
            ConcurrentDictionary<int, Foothold> fhHolder = new ConcurrentDictionary<int, Foothold>();
            Parallel.ForEach(mapEntry.Resolve("foothold").Children
                .SelectMany(c => c.Children)
                .SelectMany(c => c.Children), (fh) =>
                {
                    Foothold res = Foothold.Parse(fh);
                    fhHolder.TryAdd(res.id, res);
                });
            result.Footholds = new Dictionary<int, Foothold>(fhHolder);
            watch.Stop();
            Package.Logging($"Map ParseFootholds took {watch.ElapsedMilliseconds}");
        }

        private static void ParseInfo(Map result, WZProperty mapEntry, WZProperty mapInfo)
        {
            Stopwatch watch = Stopwatch.StartNew();

            float top = 0;
            float right = 0;
            float bottom = 0;
            float left = 0;

            Task minimapTask = Task.Run(() => result.MiniMap = MiniMap.Parse(mapEntry.Resolve("miniMap")));
            WZProperty portalProperty = mapEntry.Resolve("portal");
            WZProperty[] portals = portalProperty.Children.ToArray();
            result.portals = new Portal[portals.Length];

            Parallel.For(0, portals.Length, i =>
            {
                Portal portal = new Portal();
                portal.collection = mapEntry.FileContainer.Collection;
                WZProperty portalData = portals[i];

                foreach (WZProperty portalChildNode in portalData.Children)
                {
                    if (portalChildNode.Name == "pn") portal.PortalName = portalChildNode.ResolveForOrNull<string>();
                    if (portalChildNode.Name == "tm")
                    {
                        portal.ToMap = portalData.ResolveFor<int>() ?? int.MinValue;
                        portal.ToMapName = MapName.GetMapNameLookup(portalData)[portal.ToMap].FirstOrDefault();
                    }
                    if (portalChildNode.Name == "tn") portal.ToName = portalData.ResolveForOrNull<string>();
                    if (portalChildNode.Name == "pt") portal.Type = (PortalType)(portalData.ResolveFor<int>() ?? 0);
                    if (portalChildNode.Name == "x") portal.x = portalData.ResolveFor<int>() ?? int.MinValue;
                    if (portalChildNode.Name == "y") portal.y = portalData.ResolveFor<int>() ?? int.MinValue;
                    if (portalChildNode.Name == "image") portal.PortalName = portalData.ResolveForOrNull<string>();
                    if (portalChildNode.Name == "onlyOnce") portal.onlyOnce = portalData.ResolveFor<bool>();
                }

                if (!portal.UnknownExit)
                    portal.IsStarForcePortal = (portalData.ResolveOutlinkFor<int>($"Map/Map/Map{portal.ToMap.ToString("D8")[0]}/{portal.ToMap.ToString("D8")}.img/info/barrier") ?? 0) > 0;

                result.portals[i] = portal;
            });

            Parallel.ForEach(mapInfo.Children, (child) =>
            {
                if (child.Name == "link") result.LinksTo = child.ResolveFor<int>();
                if (child.Name == "bgm") result.BackgroundMusic = child.ResolveForOrNull<string>();
                if (child.Name == "returnMap")
                {
                    result.ReturnMap = child.ResolveFor<int>();
                    result.IsReturnMap = result.ReturnMap == 999999999;
                    if ((!result.IsReturnMap) ?? false && result.ReturnMap.HasValue)
                        result.ReturnMapName = MapName.GetMapNameLookup(mapInfo)[result.ReturnMap ?? -1].FirstOrDefault() ?? new MapName() { Id = result.ReturnMap ?? -1, Name = "Unknown", StreetName = "Unknown" };
                }
                if (child.Name == "town") result.IsTown = child.ResolveFor<bool>();
                if (child.Name == "swim") result.IsSwim = child.ResolveFor<bool>();
                if (child.Name == "mobRate") result.MobRate = child.ResolveFor<double>();
                if (child.Name == "mapMark") result.MapMark = child.ResolveForOrNull<string>();
                if (child.Name == "barrier") result.MinimumStarForce = child.ResolveFor<int>();
                if (child.Name == "barrierArc") result.MinimumArcaneForce = child.ResolveFor<int>();
                if (child.Name == "lvLimit") result.MinimumLevel = child.ResolveFor<int>();
                if (child.Name == "VRTop") top = child.ResolveFor<float>() ?? 0;
                if (child.Name == "VRRight") right = child.ResolveFor<float>() ?? 0;
                if (child.Name == "VRBottom") bottom = child.ResolveFor<float>() ?? 0;
                if (child.Name == "VRLeft") left = child.ResolveFor<float>() ?? 0;                
            });

            result.VRBounds = new RectangleF(left, top, right - left, bottom - top);
            minimapTask.Wait();
            watch.Stop();
            Package.Logging($"Map ParseInfo took {watch.ElapsedMilliseconds}");
        }

        public void ExtendFrom(Map linked)
        {
            this.Npcs = linked.Npcs;
            this.Mobs = linked.Mobs;
            this.MiniMap = linked.MiniMap;
            this.portals = linked.portals;
            this.Footholds = linked.Footholds;
        }
    }

    public class Foothold {
        public int id;
        public int next;
        public int prev;
        public int piece;
        public int x1, x2, y1, y2;
        internal static Foothold Parse(WZProperty c)
        {
            Foothold result = new Foothold();
            result.id = int.Parse(c.NameWithoutExtension);
            result.next = c.ResolveFor<int>("next") ?? 0;
            result.prev = c.ResolveFor<int>("prev") ?? 0;
            result.piece = c.ResolveFor<int>("piece") ?? 0;
            result.x1 = c.ResolveFor<int>("x1") ?? 0;
            result.x2 = c.ResolveFor<int>("x2") ?? 0;
            result.y1 = c.ResolveFor<int>("y1") ?? 0;
            result.y2 = c.ResolveFor<int>("y2") ?? 0;
            return result;
        }

        public int YAtX(int x) // If (x - x1) = 0 || (x2-x1) = 0 then by extension ((x2 - x1) * (x - x1)) = 0, and y1 should equal y2 so we can just return y1
            => (x == x1 || x2 == x1 || y2 == y1) ? y1 : (int)(y1 + ((y2 - y1) * ((x - x1) / ((x2 * 1.0f) - x1))));
    }
}
