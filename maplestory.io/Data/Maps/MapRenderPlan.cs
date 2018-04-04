using maplestory.io.Data.Images;
using Newtonsoft.Json;
using PKG1;
using SixLabors.ImageSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace maplestory.io.Data.Maps
{
    public class MapRenderPlan
    {
        public ConcurrentDictionary<string, FrameContainer> ParsedFrames = new ConcurrentDictionary<string, FrameContainer>();
        public ConcurrentDictionary<string, int> TileZIndexes = new ConcurrentDictionary<string, int>();
        Dictionary<string, ConcurrentBag<IPositionedFrameContainer>> allGraphicsParsed;
        public Dictionary<string, IPositionedFrameContainer[]> AllGraphicLayers;
        public Map map;
        [JsonIgnore]
        WZProperty mapNode;
        public MapRenderPlan(Map info, WZProperty mapNode)
        {
            map = info;
            this.mapNode = mapNode;

            IEnumerable<WZProperty> layerNodes = mapNode.Children.Where(c => int.TryParse(c.Name, out int blah)).ToArray();
            Dictionary<WZProperty, WZProperty> tileSets = layerNodes.ToDictionary(c => c.Resolve("tile"), c => c.ResolveOutlink("Map/Tile/" + c.ResolveForOrNull<string>("info/tS")));
            IEnumerable<WZProperty> allLayerNodeFolders = layerNodes.SelectMany(c => c.Children).Where(c => c.Name == "obj" || c.Name == "tile").ToArray();
            Dictionary<WZProperty, Tuple<WZProperty, WZProperty>> layersToFolders = allLayerNodeFolders.GroupBy(c => c.Parent).ToDictionary(c => c.Key, c => new Tuple<WZProperty, WZProperty>(c.First(), c.Last()));
            IEnumerable<WZProperty> allGraphics = allLayerNodeFolders.SelectMany(c => c.Children).ToArray();
            allGraphicsParsed = allLayerNodeFolders.ToDictionary(c => c.Path, c => new ConcurrentBag<IPositionedFrameContainer>());
            ConcurrentDictionary<string, int> tileZIndexes = new ConcurrentDictionary<string, int>();
            ConcurrentDictionary<string, FrameContainer> parsedFrames = new ConcurrentDictionary<string, FrameContainer>();
            ProcessGraphicsNode(tileSets, allGraphics);
            AllGraphicLayers = allGraphicsParsed.ToDictionary(c => c.Key, c =>
            {
                IPositionedFrameContainer[] positionedContainers = new IPositionedFrameContainer[c.Value.Count];
                int i = 0;
                while (c.Value.TryTake(out IPositionedFrameContainer res)) positionedContainers[i++] = res;
                return positionedContainers;
            });

            // Apparently, not clearing out ConcurrentBags is a memory leak. TIL.
            ThreadPool.QueueUserWorkItem(ClearBags, allGraphicsParsed.Values);
        }

        int lcmn(int[] numbers) => numbers.Aggregate(lcm);
        int lcm(int a, int b) => Math.Abs(a * b) / GCD(a, b);
        int GCD(int a, int b) => b == 0 ? a : GCD(b, a % b);

        void ProcessGraphicsNode(Dictionary<WZProperty, WZProperty> tileSets, IEnumerable<WZProperty> allGraphics)
        {
            Parallel.ForEach(allGraphics, node =>
            {
                if (node.Parent.Name[0] == 't')
                {
                    MapTile res = new MapTile();
                    string u = null, no = null;
                    int x = 0, y = 0;

                    foreach (WZProperty childNode in node.Children)
                    {
                        if (childNode.Name == "u") u = childNode.ResolveForOrNull<string>();
                        if (childNode.Name == "no") no = childNode.ResolveForOrNull<string>();
                        if (childNode.Name == "front") res.FrontMost = childNode.ResolveFor<bool>() ?? false;
                        if (childNode.Name == "f") res.Flip = childNode.ResolveFor<bool>() ?? false;
                        if (childNode.Name == "x") x = childNode.ResolveFor<int>() ?? 0;
                        if (childNode.Name == "y") y = childNode.ResolveFor<int>() ?? 0;
                    }

                    WZProperty tileSet = tileSets[node.Parent];
                    string elementPath = $"{tileSet.Path}/{u}/{no}";
                    if (!ParsedFrames.ContainsKey(elementPath))
                    {
                        WZProperty tileCanvas = tileSet.Resolve($"{u}/{no}")?.Resolve();
                        if (tileCanvas == null) return;
                        int tileZ = tileCanvas.ResolveFor<int>("z") ?? 1;
                        TileZIndexes.TryAdd(elementPath, tileZ);
                        FrameContainer container = new FrameContainer();
                        if (ParsedFrames.TryAdd(elementPath, container))
                        {
                            int frameCount = tileCanvas.Children?.Select(c => int.TryParse(c.Name, out int blah) ? (int?)blah : null).Where(c => c.HasValue).Select(c => c.Value).Count() ?? 1;
                            container.Frames = new Frame[frameCount];
                            for (int i = 0; i < frameCount; ++i) container.Frames[i] = Frame.Parse(tileCanvas.Resolve(i.ToString()) ?? tileCanvas);
                        }
                    }

                    res.Position = new Vector3(x, y, TileZIndexes[elementPath]);
                    res.pathToImage = elementPath;

                    allGraphicsParsed[node.Parent.Path].Add(res);
                }
                else if (node.Parent.Name[0] == 'o')
                {
                    MapObject res = new MapObject();
                    string oS = null, l0 = null, l1 = null, l2 = null;
                    int x = 0, y = 0, z = 0;

                    foreach (WZProperty childNode in node.Children)
                    {
                        if (childNode.Name == "oS") oS = childNode.ResolveForOrNull<string>();
                        if (childNode.Name == "l0") l0 = childNode.ResolveForOrNull<string>();
                        if (childNode.Name == "l1") l1 = childNode.ResolveForOrNull<string>();
                        if (childNode.Name == "l2") l2 = childNode.ResolveForOrNull<string>();
                        if (childNode.Name == "tags") res.Tags = childNode.ResolveForOrNull<string>();
                        if (childNode.Name == "front") res.FrontMost = childNode.ResolveFor<bool>() ?? false;
                        if (childNode.Name == "f") res.Flip = childNode.ResolveFor<bool>() ?? false;
                        if (childNode.Name == "x") x = childNode.ResolveFor<int>() ?? 0;
                        if (childNode.Name == "y") y = childNode.ResolveFor<int>() ?? 0;
                        if (childNode.Name == "z") z = childNode.ResolveFor<int>() ?? 0;
                        if (childNode.Name == "quest") res.Quests = childNode.Children.Select(c => int.TryParse(c.NameWithoutExtension, out int blah) ? (int?)blah : null).Where(c => c.HasValue).Select(c => c.Value).ToArray();
                    }

                    string elementPath = $"{oS}/{l0}/{l1}/{l2}";
                    res.pathToImage = elementPath;
                    res.Index = int.Parse(node.Name);
                    res.Position = new Vector3(x, y, z);

                    if (!ParsedFrames.ContainsKey(elementPath))
                    {
                        WZProperty objCanvas = node.ResolveOutlink($"Map/Obj/{elementPath}") ?? node.ResolveOutlink($"Map2/Obj/{elementPath}");
                        if (objCanvas == null) return;
                        FrameContainer container = new FrameContainer();
                        if (ParsedFrames.TryAdd(elementPath, container))
                        {
                            int frameCount = objCanvas.Children?.Select(c => int.TryParse(c.Name, out int blah) ? (int?)blah : null).Where(c => c.HasValue).Select(c => c.Value).Count() ?? 1;
                            container.Frames = new Frame[frameCount];
                            for (int i = 0; i < frameCount; ++i) container.Frames[i] = Frame.Parse(objCanvas.Resolve(i.ToString()) ?? objCanvas);
                        }
                    }

                    allGraphicsParsed[node.Parent.Path].Add(res);
                }
            });
        }

        void ClearBags(object bagState)
        {
            IEnumerable<ConcurrentBag<IPositionedFrameContainer>> bags = (IEnumerable<ConcurrentBag<IPositionedFrameContainer>>)bagState;
            foreach (ConcurrentBag<IPositionedFrameContainer> bag in bags) while (bag.TryTake(out IPositionedFrameContainer blah)) ;
        }

        public class FrameContainer
        {
            public int FrameCount { get => Frames.Length; }
            public Frame[] Frames;
        }
    }
}
