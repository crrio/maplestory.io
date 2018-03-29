﻿using MoreLinq;
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
        public IEnumerable<Portal> portals;
        public IEnumerable<MapLife> Npcs;
        public IEnumerable<MapLife> Mobs;
        public double? MobRate;
        public bool? IsTown;
        public bool? IsSwim;
        public string MapMark;
        public MiniMap MiniMap;
        public int? LinksTo;
        [JsonIgnore]
        public IEnumerable<GraphicsSet> Graphics;
        [JsonIgnore]
        public IEnumerable<MapBackground> Backgrounds;
        [JsonIgnore]
        private IEnumerable<MapLife> Life;
        public Dictionary<int, Foothold> Footholds;
        public int? MinimumStarForce;
        public int? MinimumArcaneForce;
        public int? MinimumLevel;
        public RectangleF VRBounds;
        public MapName ReturnMapName;
        public RectangleF GraphicBounds;

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
            ParseGraphics(result, mapEntry);

            return result;
        }

        private static void ParseGraphics(Map result, WZProperty mapEntry)
        {
            Stopwatch watch = Stopwatch.StartNew();
            result.Graphics = mapEntry.Children
                .Where(c => int.TryParse(c.NameWithoutExtension, out int blah))
                .AsParallel()
                .Select((c, i) => GraphicsSet.Parse(c, i)).ToArray();
            result.Backgrounds = mapEntry.Resolve("back")?.Children.AsParallel().Select(c => MapBackground.Parse(c)).ToArray();

            IEnumerable<IEnumerable<IPositionedFrameContainer>> frameContainers = result.Graphics
                .Select(g => g.Objects.Select(c => (IPositionedFrameContainer)c).Concat(g.Tiles).ToArray());
            if ((frameContainers.Count() != 0 && frameContainers.Select(c => c.Count()).Sum() != 0) && result.Backgrounds.Count() != 0)
            {
                IEnumerable<RectangleF> Bounds = frameContainers.SelectMany(c => c)
                    .Select(c => c.Bounds)
                    .Concat(result.portals.Select(c => c.Bounds))
                    .Concat(result.Life.Select(c => c.Bounds))
                    .Append(result.VRBounds)
                    .ToArray();
                float minX = Bounds.Select(c => c.X).Min();
                float maxX = Bounds.Select(c => c.X + c.Width).Max();
                float minY = Bounds.Select(c => c.Y).Min();
                float maxY = Bounds.Select(c => c.Y + c.Height).Max();
                result.GraphicBounds = new RectangleF(minX, minY, (maxX - minX), (maxY - minY));
            }

            watch.Stop();
            Package.Logging($"Map ParseGraphics took {watch.ElapsedMilliseconds}");
        }

        private static void ParseLife(Map result, WZProperty mapEntry)
        {
            Stopwatch watch = Stopwatch.StartNew();
            ConcurrentDictionary<int, Tuple<string, Frame>> lifeTemplateCache = new ConcurrentDictionary<int, Tuple<string, Frame>>();
            result.Life = mapEntry.Resolve("life")?.Children
                .GroupBy(c => c.ResolveFor<int>("id"))
                .AsParallel()
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
            result.LinksTo = mapInfo.ResolveFor<int>("link");
            result.BackgroundMusic = mapInfo.ResolveForOrNull<string>("bgm");
            result.ReturnMap = mapInfo.ResolveFor<int>("returnMap");
            //            result.IsReturnMap = result.ReturnMap == result.Id;
            result.IsReturnMap = result.ReturnMap == 999999999;
            if ((!result.IsReturnMap) ?? false && result.ReturnMap.HasValue)
                result.ReturnMapName = MapName.GetMapNameLookup(mapInfo)[result.ReturnMap ?? -1].FirstOrDefault() ?? new MapName() { Id = result.ReturnMap ?? -1, Name = "Unknown", StreetName = "Unknown" };
            result.IsTown = mapInfo.ResolveFor<bool>("town");
            result.IsSwim = mapInfo.ResolveFor<bool>("swim");
            result.MobRate = mapInfo.ResolveFor<double>("mobRate");
            result.MapMark = mapInfo.ResolveForOrNull<string>("mapMark");
            result.MinimumStarForce = mapInfo.ResolveFor<int>("barrier");
            result.MinimumArcaneForce = mapInfo.ResolveFor<int>("barrierArc");
            result.MinimumLevel = mapInfo.ResolveFor<int>("lvLimit");

            float top = mapInfo.ResolveFor<float>("VRTop") ?? 0, right = mapInfo.ResolveFor<float>("VRRight") ?? 0, bottom = mapInfo.ResolveFor<float>("VRBottom") ?? 0, left = mapInfo.ResolveFor<float>("VRLeft") ?? 0;

            result.VRBounds = new RectangleF(left, top, right - left, bottom - top);
            result.portals = mapEntry.Resolve("portal")?.Children.Select(Portal.Parse).ToArray();
            result.MiniMap = result.MiniMap = MiniMap.Parse(mapEntry.Resolve("miniMap"));
            watch.Stop();
            Package.Logging($"Map ParseInfo took {watch.ElapsedMilliseconds}");
        }

        public void ExtendFrom(Map linked)
        {
            this.Npcs = linked.Npcs;
            this.Mobs = linked.Mobs;
            this.MiniMap = linked.MiniMap;
            this.portals = linked.portals;
            this.Graphics = linked.Graphics;
            this.Backgrounds = linked.Backgrounds;
            this.Footholds = linked.Footholds;
        }

        public Image<Rgba32> Render(bool showLife, bool showPortals, bool showBackgrounds)
        {
            IEnumerable<IEnumerable<IPositionedFrameContainer>> frameContainers = Graphics
                .Select(g => g.Objects.Select(c => (IPositionedFrameContainer)c).Concat(g.Tiles).ToArray());
            if ((frameContainers.Count() == 0 || frameContainers.Select(c => c.Count()).Sum() == 0) && Backgrounds.Count() == 0) return null;
            float minX = GraphicBounds.X;
            float maxX = GraphicBounds.Width + GraphicBounds.X;
            float minY = GraphicBounds.Y;
            float maxY = GraphicBounds.Height + GraphicBounds.Y;
            ConcurrentDictionary<int, Image<Rgba32>> layers = new ConcurrentDictionary<int, Image<Rgba32>>();

            Image<Rgba32> layered = null;
            Task waitingFor = Task.Run(() => layered = showBackgrounds ? RenderBackground(this.Backgrounds, minX, minY, maxX, maxY) : new Image<Rgba32>((int)GraphicBounds.Width, (int)GraphicBounds.Height));

            if (showLife) {
                waitingFor = Task.WhenAll(waitingFor, Task.Run(() => {
                    Image<Rgba32> lifeLayer = RenderPositioned(Life, minX, minY, maxX, maxY);
                    layers.TryAdd(2000000001, lifeLayer);
                }));
            }

            if (showPortals) {
                waitingFor = Task.WhenAll(waitingFor, Task.Run(() => {
                    Image<Rgba32> lifeLayer = RenderPositioned(portals.Where(c => c.Type == PortalType.Portal), minX, minY, maxX, maxY);
                    layers.TryAdd(2000000000, lifeLayer);
                }));
            }

            while(!Parallel.ForEach(Graphics, graphicsContainer => {
                Image<Rgba32> objsLayer = RenderPositioned(
                    graphicsContainer.Objects
                        .Where(c => string.IsNullOrEmpty(c.Tags) && (c.Quests == null || c.Quests.Length == 0))
                        .OrderBy(c => c),
                    minX, minY, maxX, maxY
                );
                Image<Rgba32> tileLayer = RenderPositioned(
                    graphicsContainer.Tiles.OrderBy(c => c.Position.Z),
                    minX, minY, maxX, maxY
                );
                layers.TryAdd(graphicsContainer.Index * 2, objsLayer);
                layers.TryAdd((graphicsContainer.Index * 2) + 1, tileLayer);
            }).IsCompleted) Thread.Sleep(1);

            if (waitingFor != null) Task.WaitAll(waitingFor);

            layered
                .Mutate(x => layers
                    .OrderBy(c => c.Key)
                    .Select(c => c.Value)
                    .ForEach(layer => x.DrawImage(layer, 1, new Size(layered.Width, layered.Height), new Point(0, 0))));

            return layered;
        }

        private Image<Rgba32> RenderBackground(IEnumerable<MapBackground> backgrounds, float minX, float minY, float maxX, float maxY)
        {
            Image<Rgba32> layerResult = new Image<Rgba32>((int)(maxX - minX), (int)(maxY - minY));
            Rectangle layerBounds = new Rectangle(0, 0, layerResult.Width, layerResult.Height);
            foreach (MapBackground frameContainer in backgrounds.Where(c => c?.Canvas?.Image != null).OrderBy(c => c.Position.Z))
            {
                Point origin = frameContainer.Canvas.Origin ?? (new Point(frameContainer.Canvas.Image.Width / 2, frameContainer.Canvas.Image.Height / 2));
                Point drawAt = new Point(
                    (int)((frameContainer.Position.X - origin.X) - minX),
                    (int)((frameContainer.Position.Y - origin.Y) - minY)
                );
                Size frameSize = new Size(frameContainer.Canvas.Image.Width, frameContainer.Canvas.Image.Height);

                switch (frameContainer.Type)
                {
                    case BackgroundType.Single:
                        if (layerBounds.IntersectsWith(new Rectangle(drawAt, frameSize)))
                            layerResult.Mutate(x => x.DrawImage(
                                frameContainer.Canvas.Image, frameContainer.Alpha,
                                new Size(frameContainer.Canvas.Image.Width, frameContainer.Canvas.Image.Height),
                                drawAt
                            ));
                        break;

                    case BackgroundType.ScrollingTiledHorizontal:
                        for (int x = (int)minX; x < maxX; x += frameContainer.Canvas.Image.Width)
                        {
                            drawAt = new Point((int)(x - minX), drawAt.Y);
                            if (layerBounds.IntersectsWith(new Rectangle(drawAt, frameSize)))
                                layerResult.Mutate(y => y.DrawImage(
                                    frameContainer.Canvas.Image, frameContainer.Alpha,
                                    new Size(frameContainer.Canvas.Image.Width, frameContainer.Canvas.Image.Height),
                                    drawAt
                                ));
                        }
                        break;

                    case BackgroundType.TiledVertical:
                    case BackgroundType.ScrollingTiledVertical:
                        for (int y = (int)minY; y < maxY; y += frameContainer.Canvas.Image.Height)
                        {
                            drawAt = new Point(drawAt.X, (int)(y - minY));
                            if (layerBounds.IntersectsWith(new Rectangle(drawAt, frameSize)))
                                layerResult.Mutate(x => x.DrawImage(
                                    frameContainer.Canvas.Image, frameContainer.Alpha,
                                    new Size(frameContainer.Canvas.Image.Width, frameContainer.Canvas.Image.Height),
                                    drawAt
                                ));
                        }
                        break;

                    case BackgroundType.ScrollingHorizontalTiledBoth:
                    case BackgroundType.ScrollingVerticalTiledBoth:
                    case BackgroundType.TiledBoth:
                        for (int x = (int)minX; x < maxX; x += frameContainer.Canvas.Image.Width)
                            for (int y = (int)minY; y < maxY; y += frameContainer.Canvas.Image.Height)
                            {
                                drawAt = new Point((int)(x - minX), (int)(y - minY));
                                if (layerBounds.IntersectsWith(new Rectangle(drawAt, frameSize)))
                                    layerResult.Mutate(k => k.DrawImage(
                                        frameContainer.Canvas.Image, frameContainer.Alpha,
                                        new Size(frameContainer.Canvas.Image.Width, frameContainer.Canvas.Image.Height),
                                        drawAt
                                    ));
                            }
                        break;
                }
            }
            return layerResult;
        }

        Image<Rgba32> RenderPositioned(IEnumerable<IPositionedFrameContainer> frameContainerZ, float minX, float minY, float maxX, float maxY) {
            Image<Rgba32> layerResult = new Image<Rgba32>((int)(maxX - minX), (int)(maxY - minY));
            Rectangle layerBounds = new Rectangle(0, 0, layerResult.Width, layerResult.Height);
            foreach (IPositionedFrameContainer frameContainer in frameContainerZ.Where(c => c?.Canvas?.Image != null))
            {
                Point origin = frameContainer.Canvas.Origin ?? (new Point(frameContainer.Canvas.Image.Width / 2, frameContainer.Canvas.Image.Height / 2));
                Point drawAt = new Point(
                    (int)((frameContainer.Position.X - origin.X) - minX),
                    (int)((frameContainer.Position.Y - origin.Y) - minY)
                );
                if (!layerBounds.Contains(drawAt)) continue;
                layerResult.Mutate(c => c.DrawImage(
                    frameContainer.Canvas.Image, 1,
                    new Size(frameContainer.Canvas.Image.Width, frameContainer.Canvas.Image.Height),
                    drawAt
                ));
                // Draw NPC Names at some point
            }
            return layerResult;
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