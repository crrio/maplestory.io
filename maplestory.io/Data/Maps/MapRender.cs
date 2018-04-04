using maplestory.io.Data.Images;
using MoreLinq;
using PKG1;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using SixLabors.Shapes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace maplestory.io.Data.Maps
{
    public class MapRender
    {
        public IEnumerable<GraphicsSet> Graphics;
        public IEnumerable<MapBackground> Backgrounds;

        Map map;
        WZProperty mapNode;
        int minX, minY, maxX, maxY;
        public MapRender(Map info,  WZProperty mapNode)
        {
            map = info;
            this.mapNode = mapNode;
        }

        //private static void ParseGraphics(Map result, WZProperty mapEntry)
        //{
        //    Stopwatch watch = Stopwatch.StartNew();

        //    ConcurrentDictionary<int, GraphicsSet> graphics = new ConcurrentDictionary<int, GraphicsSet>();
        //    Parallel.ForEach(mapEntry.Children, child =>
        //    {
        //        if (int.TryParse(child.Name, out int index))
        //            graphics.TryAdd(index, GraphicsSet.Parse(child, index));
        //        if (child.Name == "back")
        //            result.Backgrounds = child.Children.Select(c => MapBackground.Parse(c)).ToArray()
        //    });
        //    result.Graphics = graphics.OrderBy(c => c.Key).Select(c => c.Value).ToArray();
        //    result.Backgrounds = mapEntry.Resolve("back")?.Children.Select(c => MapBackground.Parse(c)).ToArray();

        //    IEnumerable<IEnumerable<IPositionedFrameContainer>> frameContainers = result.Graphics
        //        .Select(g => g.Objects.Select(c => (IPositionedFrameContainer)c).Concat(g.Tiles).ToArray());
        //    if ((frameContainers.Count() != 0 && frameContainers.Select(c => c.Count()).Sum() != 0) && result.Backgrounds.Count() != 0)
        //    {
        //        IEnumerable<RectangleF> Bounds = frameContainers.SelectMany(c => c)
        //            .Select(c => c.Bounds)
        //            .Concat(result.portals.Select(c => c.Bounds))
        //            .Concat(result.Life.Select(c => c.Bounds))
        //            .Append(result.VRBounds)
        //            .ToArray();
        //        float minX = Bounds.Select(c => c.X).Min();
        //        float maxX = Bounds.Select(c => c.X + c.Width).Max();
        //        float minY = Bounds.Select(c => c.Y).Min();
        //        float maxY = Bounds.Select(c => c.Y + c.Height).Max();
        //        result.GraphicBounds = new RectangleF(minX, minY, (maxX - minX), (maxY - minY));
        //    }

        //    watch.Stop();
        //    Package.Logging($"Map ParseGraphics took {watch.ElapsedMilliseconds}");
        //}

        int lcmn(int[] numbers) => numbers.Aggregate(lcm);
        int lcm(int a, int b) => Math.Abs(a * b) / GCD(a, b);
        int GCD(int a, int b) => b == 0 ? a : GCD(b, a % b);

        void ProcessGraphicsNode(int frame, Dictionary<WZProperty, WZProperty> tileSets, IEnumerable<WZProperty> allGraphics, Dictionary<WZProperty, ConcurrentBag<IPositionedFrameContainer>> allGraphicsParsed, bool filterTrash = false)
        {
            ConcurrentDictionary<string, Frame> parsedFrames = new ConcurrentDictionary<string, Frame>();
            ConcurrentDictionary<string, Frame> parsedFlippedFrames = new ConcurrentDictionary<string, Frame>();
            ConcurrentDictionary<string, int> tileZIndexes = new ConcurrentDictionary<string, int>();
            ConcurrentBag<int> allX = new ConcurrentBag<int>();
            ConcurrentBag<int> allRight = new ConcurrentBag<int>();
            ConcurrentBag<int> allY = new ConcurrentBag<int>();
            ConcurrentBag<int> allBottom = new ConcurrentBag<int>();
            ConcurrentDictionary<WZProperty, ConcurrentBag<int>> frameCounts = new ConcurrentDictionary<WZProperty, ConcurrentBag<int>>();
            Parallel.ForEach(allGraphics, node =>
            {
                if (node.Parent.Name[0] == 't')
                {
                    string u = null, no = null;
                    bool frontMost = false, flip = false;
                    int x = 0, y = 0, z = 0;

                    foreach (WZProperty childNode in node.Children)
                    {
                        if (childNode.Name == "u") u = childNode.ResolveForOrNull<string>();
                        if (childNode.Name == "no") no = childNode.ResolveForOrNull<string>();
                        if (childNode.Name == "front") frontMost = childNode.ResolveFor<bool>() ?? false;
                        if (childNode.Name == "f") flip = childNode.ResolveFor<bool>() ?? false;
                        if (childNode.Name == "x") x = childNode.ResolveFor<int>() ?? 0;
                        if (childNode.Name == "y") y = childNode.ResolveFor<int>() ?? 0;
                    }

                    WZProperty tileSet = tileSets[node.Parent];
                    WZProperty canvasNode = null;
                    int frameCount = 0;

                    string elementPath = $"{tileSet.Path}/{u}/{no}";
                    Frame parsedFrame = null;

                    bool created = false;
                    if (parsedFrames.ContainsKey(elementPath) && (!flip || !parsedFlippedFrames.ContainsKey(elementPath)))
                    {
                        parsedFrame = parsedFrames[elementPath];

                        if (flip)
                        {
                            parsedFrame = new Frame()
                            {
                                delay = parsedFrame.delay,
                                MapOffset = parsedFrame.MapOffset,
                                Origin = parsedFrame.Origin,
                                Position = parsedFrame.Position,
                                Image = parsedFrame.Image.Clone(c => c.Flip(FlipType.Horizontal))
                            };
                            parsedFlippedFrames.TryAdd(elementPath, parsedFrame);
                        }
                    }
                    else if (parsedFlippedFrames.ContainsKey(elementPath) && flip)
                        parsedFrame = parsedFlippedFrames[elementPath];
                    else
                    {
                        created = true;
                        WZProperty tileCanvas = tileSet.Resolve($"{u}/{no}")?.Resolve();
                        if (tileCanvas == null) return;
                        int tileZ = tileCanvas.ResolveFor<int>("z") ?? 1;
                        frameCount = tileCanvas.Children?.Select(c => int.TryParse(c.Name, out int blah) ? (int?)blah : null).Where(c => c.HasValue).Select(c => c.Value).Count() ?? 1;
                        canvasNode = tileCanvas.Resolve((frameCount == 0 ? 0 : (frame % frameCount)).ToString()) ?? tileCanvas;
                        parsedFrame = Frame.Parse(canvasNode);
#if DEBUG
                        if (!frameCounts.ContainsKey(node.Parent))
                            frameCounts.TryAdd(node.Parent, new ConcurrentBag<int>());
                        frameCounts[node.Parent].Add(frameCount);
#endif

                        tileZIndexes.TryAdd(elementPath, tileZ);
                        if (flip)
                        {
                            parsedFrame.Image = parsedFrame.Image.Clone(c => c.Flip(FlipType.Horizontal));
                            parsedFlippedFrames.TryAdd(elementPath, parsedFrame);
                        }
                        else parsedFrames.TryAdd(elementPath, parsedFrame);
                    }

                    z = tileZIndexes[elementPath];

                    int rightX = x + parsedFrame.Image.Width;
                    int bottomY = y + parsedFrame.Image.Height;
                    allX.Add(x);
                    allY.Add(y);
                    allRight.Add(rightX);
                    allBottom.Add(bottomY);

                    allGraphicsParsed[node.Parent].Add(new MapTile()
                    {
                        Canvas = parsedFrame,
                        Flip = flip,
                        FrontMost = frontMost,
                        pathToImage = elementPath,
                        Position = new Vector3(x, y, z)
                    });
                }
                else if (node.Parent.Name[0] == 'o')
                {
                    string oS = null, l0 = null, l1 = null, l2 = null, tags = null;
                    bool frontMost = false, flip = false;
                    int x = 0, y = 0, z = 0;
                    float r = 0;
                    int[] quests = null;

                    foreach (WZProperty childNode in node.Children)
                    {
                        if (childNode.Name == "oS") oS = childNode.ResolveForOrNull<string>();
                        if (childNode.Name == "l0") l0 = childNode.ResolveForOrNull<string>();
                        if (childNode.Name == "l1") l1 = childNode.ResolveForOrNull<string>();
                        if (childNode.Name == "l2") l2 = childNode.ResolveForOrNull<string>();
                        if (childNode.Name == "tags") return;//tags = childNode.ResolveForOrNull<string>();
                        if (childNode.Name == "front") frontMost = childNode.ResolveFor<bool>() ?? false;
                        if (childNode.Name == "f") flip = childNode.ResolveFor<bool>() ?? false;
                        if (childNode.Name == "x") x = childNode.ResolveFor<int>() ?? 0;
                        if (childNode.Name == "y") y = childNode.ResolveFor<int>() ?? 0;
                        if (childNode.Name == "z") z = childNode.ResolveFor<int>() ?? 0;
                        if (childNode.Name == "quest") return;//quests = childNode.Children.Select(c => int.TryParse(c.NameWithoutExtension, out int blah) ? (int?)blah : null).Where(c => c.HasValue).Select(c => c.Value).ToArray();
                    }

                    if (filterTrash && (oS == "MFF" || l0 == "2011Xmas")) return;

                    string elementPath = $"{oS}/{l0}/{l1}/{l2}";

                    bool created = false;
                    Frame parsedFrame = null;
                    int frameCount = 0;
                    if (parsedFrames.ContainsKey(elementPath) && (!flip || !parsedFlippedFrames.ContainsKey(elementPath)))
                    {
                        parsedFrame = parsedFrames[elementPath];

                        if (flip)
                        {
                            parsedFrame = new Frame()
                            {
                                delay = parsedFrame.delay,
                                MapOffset = parsedFrame.MapOffset,
                                Origin = parsedFrame.Origin,
                                Position = parsedFrame.Position,
                                Image = parsedFrame.Image.Clone(c => c.Flip(FlipType.Horizontal))
                            };
                            parsedFlippedFrames.TryAdd(elementPath, parsedFrame);
                        }
                    }
                    else if (parsedFlippedFrames.ContainsKey(elementPath) && flip)
                        parsedFrame = parsedFlippedFrames[elementPath];
                    else
                    {
                        created = true;
                        WZProperty objCanvas = node.ResolveOutlink($"Map/Obj/{elementPath}") ?? node.ResolveOutlink($"Map2/Obj/{elementPath}");
                        if (objCanvas == null) return;
                        frameCount = objCanvas.Children?.Select(c => int.TryParse(c.Name, out int blah) ? (int?)blah : null).Where(c => c.HasValue).Select(c => c.Value).Count() ?? 1;
#if DEBUG
                        if (!frameCounts.ContainsKey(node.Parent))
                            frameCounts.TryAdd(node.Parent, new ConcurrentBag<int>());
                        frameCounts[node.Parent].Add(frameCount);
                        parsedFrame = Frame.Parse(objCanvas.Resolve((frameCount == 0 ? 0 : (frame % frameCount)).ToString()) ?? objCanvas);
#endif

                        if (flip)
                        {
                            parsedFrame.Image = parsedFrame.Image.Clone(c => c.Flip(FlipType.Horizontal));
                            parsedFlippedFrames.TryAdd(elementPath, parsedFrame);
                        }
                        else parsedFrames.TryAdd(elementPath, parsedFrame);
                    }

                    int visualX = x - parsedFrame.OriginOrZero.X;
                    int visualY = y - parsedFrame.OriginOrZero.Y;
                    int rightX = visualX + parsedFrame.Image.Width;
                    int bottomY = visualY + parsedFrame.Image.Height;
                    allX.Add(visualX);
                    allY.Add(visualY);
                    allRight.Add(rightX);
                    allBottom.Add(bottomY);

                    allGraphicsParsed[node.Parent].Add(new MapObject()
                    {
                        Canvas = parsedFrame,
                        Flip = flip,
                        FrontMost = frontMost,
                        pathToImage = elementPath,
                        Position = new Vector3(x, y, z),
                        Quests = quests,
                        Rotation = r,
                        Tags = tags
                    });
                }
            });

            minX = allX.Min();
            maxX = allRight.Max();
            minY = allY.Min();
            maxY = allBottom.Max();
#if DEBUG
            Dictionary<string, int> idealFrameCounts = frameCounts.ToDictionary(c => c.Key.Path, c =>
            {
                int[] layerFrameCounts = new int[c.Value.Count];
                int i = 0;
                while (c.Value.TryTake(out int frameCount)) layerFrameCounts[i++] = frameCount;
                return lcmn(layerFrameCounts.Where(b => b != 0).DefaultIfEmpty(1).ToArray());
            });
#endif

            ThreadPool.QueueUserWorkItem(s =>
            {
                foreach (ConcurrentBag<int> bag in (ConcurrentBag<int>[])s) while (bag.TryTake(out int blah)) ;
            }, new ConcurrentBag<int>[] { allX, allY, allRight, allBottom });
        }

        public Image<Rgba32> RenderLayer(int frame, int layer, bool filterTrash = false)
        {
            int folderNumber = layer / 2;
            bool isObjLayer = layer % 2 == 0;
            IEnumerable<WZProperty> layerNodes = mapNode.Children.Where(c => int.TryParse(c.Name, out int blah) && blah == folderNumber).ToArray();
            Dictionary<WZProperty, WZProperty> tileSets = layerNodes.ToDictionary(c => c.Resolve("tile"), c => c.ResolveOutlink("Map/Tile/" + c.ResolveForOrNull<string>("info/tS")));
            IEnumerable<WZProperty> allLayerNodeFolders = layerNodes.SelectMany(c => c.Children).Where(c => (isObjLayer && c.Name == "obj") || (!isObjLayer && c.Name == "tile")).ToArray();
            Dictionary<WZProperty, Tuple<WZProperty, WZProperty>> layersToFolders = allLayerNodeFolders.GroupBy(c => c.Parent).ToDictionary(c => c.Key, c => new Tuple<WZProperty, WZProperty>(c.First(), c.Last()));
            IEnumerable<WZProperty> allGraphics = allLayerNodeFolders.SelectMany(c => c.Children).ToArray();
            Dictionary<WZProperty, ConcurrentBag<IPositionedFrameContainer>> allGraphicsParsed = allLayerNodeFolders.ToDictionary(c => c, c => new ConcurrentBag<IPositionedFrameContainer>());
            ConcurrentDictionary<int, Image<Rgba32>> renderedLayers = new ConcurrentDictionary<int, Image<Rgba32>>();

            ProcessGraphicsNode(frame, tileSets, allGraphics, allGraphicsParsed, filterTrash);
            Parallel.ForEach(allGraphicsParsed.Where(c => c.Value.Count > 0), layerElements => renderedLayers.TryAdd(GetLayerIndex(layerElements.Key), RenderPositioned(layerElements.Value)));

            return renderedLayers[layer];
        }

        public Image<Rgba32> Render(int frame, bool showLife, bool showPortals, bool showBackgrounds)
        {
            IEnumerable<WZProperty> layerNodes = mapNode.Children.Where(c => int.TryParse(c.Name, out int blah)).ToArray();
            Dictionary<WZProperty, WZProperty> tileSets = layerNodes.ToDictionary(c => c.Resolve("tile"), c => c.ResolveOutlink("Map/Tile/" + c.ResolveForOrNull<string>("info/tS")));
            IEnumerable<WZProperty> allLayerNodeFolders = layerNodes.SelectMany(c => c.Children).Where(c => c.Name == "obj" || c.Name == "tile").ToArray();
            Dictionary<WZProperty, Tuple<WZProperty, WZProperty>> layersToFolders = allLayerNodeFolders.GroupBy(c => c.Parent).ToDictionary(c => c.Key, c => new Tuple<WZProperty, WZProperty>(c.First(), c.Last()));
            IEnumerable<WZProperty> allGraphics = allLayerNodeFolders.SelectMany(c => c.Children).ToArray();
            Dictionary<WZProperty, ConcurrentBag<IPositionedFrameContainer>> allGraphicsParsed = allLayerNodeFolders.ToDictionary(c => c, c => new ConcurrentBag<IPositionedFrameContainer>());
            ConcurrentDictionary<int, Image<Rgba32>> renderedLayers = new ConcurrentDictionary<int, Image<Rgba32>>();

            ProcessGraphicsNode(frame, tileSets, allGraphics, allGraphicsParsed);

            List<Task> otherLayers = new List<Task>();
            if (showLife)
            {
                otherLayers.Add(Task.Run(() => renderedLayers.TryAdd(2000000001, RenderPositioned(map.Life))));
                otherLayers.Add(Task.Run(() => renderedLayers.TryAdd(2000000002, RenderLifeNames(map.Life))));
            }
            if (showPortals) otherLayers.Add(Task.Run(() => renderedLayers.TryAdd(2000000003, RenderPositioned(map.portals.Where(c => c.Type == PortalType.Portal)))));

            Parallel.ForEach(allGraphicsParsed.Where(c => c.Value.Count > 0), layerElements => renderedLayers.TryAdd(GetLayerIndex(layerElements.Key), RenderPositioned(layerElements.Value)));
            Task.WaitAll(otherLayers.ToArray());

            int width = maxX - minX;
            int height = maxY - minY;

            Image<Rgba32> layered = new Image<Rgba32>(width, height);
            layered.Mutate(
                x => renderedLayers
                        .OrderBy(c => c.Key)
                        .Select(c => c.Value)
                        .ForEach(
                            layer => x.DrawImage(layer, 1, new Size(width, height), new Point(0, 0))
                        )
            );

            ThreadPool.QueueUserWorkItem(DisposeLayers, renderedLayers.Values);
            // Apparently, not clearing out ConcurrentBags is a memory leak. TIL.
            ThreadPool.QueueUserWorkItem(ClearBags, allGraphicsParsed.Values);
            return layered;
        }

        IPathCollection BuildCorners(int x, int y, int width, int height, float cornerRadius)
        {
            // first create a square
            var rect = new RectangularePolygon(x - 0.5f, y - 0.5f, cornerRadius, cornerRadius);

            // then cut out of the square a circle so we are left with a corner
            IPath cornerToptLeft = rect.Clip(new EllipsePolygon(x + (cornerRadius - 0.5f), y + (cornerRadius - 0.5f), cornerRadius));

            // corner is now a corner shape positions top left
            //lets make 3 more positioned correctly, we can do that by translating the orgional artound the center of the image
            var center = new Vector2(width / 2F, height / 2F);

            float rightPos = width - cornerToptLeft.Bounds.Width + 1;
            float bottomPos = height - cornerToptLeft.Bounds.Height + 1;

            // move it across the widthof the image - the width of the shape
            IPath cornerTopRight = cornerToptLeft.RotateDegree(90).Translate(rightPos, 0);
            IPath cornerBottomLeft = cornerToptLeft.RotateDegree(-90).Translate(0, bottomPos);
            IPath cornerBottomRight = cornerToptLeft.RotateDegree(180).Translate(rightPos, bottomPos);

            return new PathCollection(cornerToptLeft, cornerBottomLeft, cornerTopRight, cornerBottomRight);
        }


        private Image<Rgba32> RenderLifeNames(IEnumerable<MapLife> life)
        {
            Image<Rgba32> layer = new Image<Rgba32>((maxX - minX), (maxY - minY));

            layer.Mutate(x =>
            {
                Font MaplestoryFont = Characters.CharacterAvatar.fonts.Families.First(f => f.Name.Equals("Arial", StringComparison.CurrentCultureIgnoreCase)).CreateFont(12, FontStyle.Regular);
                foreach (MapLife npc in life)
                {
                    SizeF nameSize = TextMeasurer.Measure(npc.Name, new RendererOptions(MaplestoryFont));
                    Rectangle boxPosition = new Rectangle((int)((npc.Position.X - (nameSize.Width / 2)) - 2) - minX, (int)(npc.Position.Y - minY) + 5, (int)nameSize.Width + 5, (int)nameSize.Height + 4);
                    x.Fill(new Rgba32(0, 0, 0, 128), boxPosition);
                    IPathCollection iPath = BuildCorners(boxPosition.X, boxPosition.Y, boxPosition.Width, boxPosition.Height, 4);
                    x.Fill(new Rgba32(0, 0, 0, 0), iPath, new GraphicsOptions() { BlenderMode = PixelBlenderMode.Src });
                    x.DrawText(npc.Name, MaplestoryFont, new Rgba32(223, 220, 109, byte.MaxValue), new PointF(boxPosition.X + 2, boxPosition.Y - 1));
                }
            });

            return layer;
        }

        void DisposeLayers(object layersState)
        {
            IEnumerable<Image<Rgba32>> layers = (IEnumerable<Image<Rgba32>>)layersState;
            foreach (Image<Rgba32> layer in layers) layer.Dispose();
        }

        void ClearBags(object bagState)
        {
            IEnumerable<ConcurrentBag<IPositionedFrameContainer>> bags = (IEnumerable<ConcurrentBag<IPositionedFrameContainer>>)bagState;
            foreach (ConcurrentBag<IPositionedFrameContainer> bag in bags) while (bag.TryTake(out IPositionedFrameContainer blah)) ;
        }

        int GetLayerIndex(WZProperty node) => (int.Parse(node.Parent.Name) * 2) + (node.Name[0] == 't' ? 1 : 0);

        //private Image<Rgba32> RenderBackground(IEnumerable<MapBackground> backgrounds, float minX, float minY, float maxX, float maxY)
        //{
        //    Image<Rgba32> layerResult = new Image<Rgba32>((int)(maxX - minX), (int)(maxY - minY));
        //    Rectangle layerBounds = new Rectangle(0, 0, layerResult.Width, layerResult.Height);
        //    foreach (MapBackground frameContainer in backgrounds.Where(c => c?.Canvas?.Image != null).OrderBy(c => c.Position.Z))
        //    {
        //        Point origin = frameContainer.Canvas.Origin ?? (new Point(frameContainer.Canvas.Image.Width / 2, frameContainer.Canvas.Image.Height / 2));
        //        Point drawAt = new Point(
        //            (int)((frameContainer.Position.X - origin.X) - minX),
        //            (int)((frameContainer.Position.Y - origin.Y) - minY)
        //        );
        //        Size frameSize = new Size(frameContainer.Canvas.Image.Width, frameContainer.Canvas.Image.Height);

        //        switch (frameContainer.Type)
        //        {
        //            case BackgroundType.Single:
        //                if (layerBounds.IntersectsWith(new Rectangle(drawAt, frameSize)))
        //                    layerResult.Mutate(x => x.DrawImage(
        //                        frameContainer.Canvas.Image, frameContainer.Alpha,
        //                        new Size(frameContainer.Canvas.Image.Width, frameContainer.Canvas.Image.Height),
        //                        drawAt
        //                    ));
        //                break;

        //            case BackgroundType.ScrollingTiledHorizontal:
        //                for (int x = (int)minX; x < maxX; x += frameContainer.Canvas.Image.Width)
        //                {
        //                    drawAt = new Point((int)(x - minX), drawAt.Y);
        //                    if (layerBounds.IntersectsWith(new Rectangle(drawAt, frameSize)))
        //                        layerResult.Mutate(y => y.DrawImage(
        //                            frameContainer.Canvas.Image, frameContainer.Alpha,
        //                            new Size(frameContainer.Canvas.Image.Width, frameContainer.Canvas.Image.Height),
        //                            drawAt
        //                        ));
        //                }
        //                break;

        //            case BackgroundType.TiledVertical:
        //            case BackgroundType.ScrollingTiledVertical:
        //                for (int y = (int)minY; y < maxY; y += frameContainer.Canvas.Image.Height)
        //                {
        //                    drawAt = new Point(drawAt.X, (int)(y - minY));
        //                    if (layerBounds.IntersectsWith(new Rectangle(drawAt, frameSize)))
        //                        layerResult.Mutate(x => x.DrawImage(
        //                            frameContainer.Canvas.Image, frameContainer.Alpha,
        //                            new Size(frameContainer.Canvas.Image.Width, frameContainer.Canvas.Image.Height),
        //                            drawAt
        //                        ));
        //                }
        //                break;

        //            case BackgroundType.ScrollingHorizontalTiledBoth:
        //            case BackgroundType.ScrollingVerticalTiledBoth:
        //            case BackgroundType.TiledBoth:
        //                for (int x = (int)minX; x < maxX; x += frameContainer.Canvas.Image.Width)
        //                    for (int y = (int)minY; y < maxY; y += frameContainer.Canvas.Image.Height)
        //                    {
        //                        drawAt = new Point((int)(x - minX), (int)(y - minY));
        //                        if (layerBounds.IntersectsWith(new Rectangle(drawAt, frameSize)))
        //                            layerResult.Mutate(k => k.DrawImage(
        //                                frameContainer.Canvas.Image, frameContainer.Alpha,
        //                                new Size(frameContainer.Canvas.Image.Width, frameContainer.Canvas.Image.Height),
        //                                drawAt
        //                            ));
        //                    }
        //                break;
        //        }
        //    }
        //    return layerResult;
        //}

        Image<Rgba32> RenderPositioned(IEnumerable<IPositionedFrameContainer> frameContainerZ)
        {
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
}
