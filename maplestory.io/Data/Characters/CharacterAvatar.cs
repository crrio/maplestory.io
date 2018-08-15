using maplestory.io.Data.Images;
using maplestory.io.Models;
using MoreLinq;
using PKG1;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Drawing;
using SixLabors.ImageSharp.Processing.Filters;
using SixLabors.ImageSharp.Processing.Text;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.ImageSharp.Processing.Transforms.Resamplers;
using SixLabors.Primitives;
using SixLabors.Shapes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace maplestory.io.Data.Characters
{
    public class CharacterAvatar
    {
        public int SkinId;
        public EquipSelection[] Equips;
        public RenderMode Mode;
        public int FrameNumber;
        public string AnimationName;
        public Dictionary<string, int> FrameCounts;
        public Dictionary<string, int[]> FrameDelays;
        private readonly MSPackageCollection wz;
        public int Padding;
        public bool ElfEars;
        public bool LefEars;
        private int weaponType;
        private Dictionary<string, string> smap;
        private Dictionary<string, int> exclusiveLocks;
        private List<string> zmap;
        public Tuple<WZProperty, EquipSelection>[] equipped;
        private bool preloaded;
        private WZProperty body;
        public string Name;
        public float Zoom;
        public bool FlipX;
        public float NameWidthAdjustmentX;
        public bool HasChair = false;
        public bool HasMount = false;

        internal static FontCollection fonts;
        private string chairSitAction = "sit";

        static CharacterAvatar()
        {
            fonts = new FontCollection();
            using (FileStream arial = File.OpenRead("assets/Fonts/arial.ttf"))
                fonts.Install(arial);
        }

        public CharacterAvatar(MSPackageCollection wz)
        {
            this.wz = wz;
        }
        public CharacterAvatar(CharacterAvatar old)
        {
            this.body = old.body;
            this.SkinId = old.SkinId;
            this.Equips = old.Equips;
            this.Mode = old.Mode;
            this.FrameNumber = old.FrameNumber;
            this.AnimationName = old.AnimationName;
            this.wz = old.wz;
            this.Padding = old.Padding;
            this.ElfEars = old.ElfEars;
            this.LefEars = old.LefEars;
            this.weaponType = old.weaponType;
            this.smap = old.smap;
            this.exclusiveLocks = old.exclusiveLocks;
            this.zmap = old.zmap;
            this.equipped = old.equipped;
            this.preloaded = old.preloaded;
            this.FrameCounts = old.FrameCounts;
            this.FrameDelays = old.FrameDelays;
            this.HasChair = old.HasChair;
            this.HasMount = old.HasMount;
            this.chairSitAction = old.chairSitAction;
        }

        public Tuple<Frame, Point, float?>[] GetFrameParts(Dictionary<string, Point> anchorPositions = null)
        {
            List<KeyValuePair<string, Point>[]> offsets = new List<KeyValuePair<string, Point>[]>();
            RankedFrame[] partsData = GetAnimationParts(offsets).OrderBy(c => c.ranking).ToArray();
            Tuple<Frame, float?>[] partsFrames = partsData.Select(c => new Tuple<Frame, float?>(c.frame, c.underlyingEquip.Hue)).ToArray();

            if (anchorPositions == null) anchorPositions = new Dictionary<string, Point>() { { "navel", new Point(0, 0) } };
            else if (!anchorPositions.ContainsKey("navel")) anchorPositions.Add("navel", new Point(0, 0));
            RankedFrame bodyFrame = partsData.FirstOrDefault(c => (c.frame.Position == "body" || c.frame.Position == "backBody") && c.frame.MapOffset.ContainsKey("neck") && c.frame.MapOffset.ContainsKey("navel"));
            Point neckOffsetBody = bodyFrame.frame.MapOffset["neck"];
            Point navelOffsetBody = bodyFrame.frame.MapOffset["navel"];

            if (AnimationName.Equals("alert", StringComparison.CurrentCultureIgnoreCase))
            {
                switch (FrameNumber % 3)
                {
                    case 0:
                        anchorPositions.Add("handMove", new Point(-8, -2));
                        break;
                    case 1:
                        anchorPositions.Add("handMove", new Point(-10, 0));
                        break;
                    case 2:
                        anchorPositions.Add("handMove", new Point(-12, 3));
                        break;
                }
            }

            offsets.RemoveAll(c => c == null);
            while (offsets.Count > 0)
            {
                KeyValuePair<string, Point>[] offsetPairing = offsets.FirstOrDefault(c => c.Any(b => anchorPositions.ContainsKey(b.Key)));
                if (offsetPairing == null) break;
                KeyValuePair<string, Point> anchorPointEntry = offsetPairing.Where(c => anchorPositions.ContainsKey(c.Key)).FirstOrDefault();
                // Handle alert position? How to :<
                Point anchorPoint = anchorPoint = anchorPositions[anchorPointEntry.Key];
                Point vectorFromPoint = anchorPointEntry.Value;
                if (Math.Abs(vectorFromPoint.X) == 999999 || Math.Abs(vectorFromPoint.Y) == 999999) vectorFromPoint = new Point(0, 0); // TODO: Figure out what '999999' is supposed to do D:
                Point fromAnchorPoint = new Point(anchorPoint.X - vectorFromPoint.X, anchorPoint.Y - vectorFromPoint.Y);

                foreach (KeyValuePair<string, Point> childAnchorPoint in offsetPairing.Where(c => c.Key != anchorPointEntry.Key))
                    if (!anchorPositions.ContainsKey(childAnchorPoint.Key))
                        anchorPositions.Add(childAnchorPoint.Key, new Point(fromAnchorPoint.X + childAnchorPoint.Value.X, fromAnchorPoint.Y + childAnchorPoint.Value.Y));

                offsets.Remove(offsetPairing);
            }

            Tuple<Frame, Point, float?>[] positionedFrames = partsFrames.Select(c =>
            {
                // Some effects are centered off of the neck
                Point fromAnchorPoint = neckOffsetBody;
                if (c.Item1.MapOffset != null)
                {
                    // Some effects are centered on the origin (0,0)
                    if (c.Item1.MapOffset.All(b => b.Key.Equals("zero")))
                    {
                        fromAnchorPoint = new Point(-navelOffsetBody.X, -navelOffsetBody.Y);
                    }
                    else
                    { // Default positioning based off of offsets
                        KeyValuePair<string, Point> anchorPointEntry = (c.Item1.MapOffset ?? new Dictionary<string, Point>()).Where(b => b.Key != null && anchorPositions.ContainsKey(b.Key)).DefaultIfEmpty(new KeyValuePair<string, Point>(null, Point.Empty)).First();
                        if (anchorPointEntry.Key == null) return null;
                        Point anchorPoint = anchorPoint = anchorPositions[anchorPointEntry.Key];
                        Point vectorFromPoint = anchorPointEntry.Value;
                        if (Math.Abs(vectorFromPoint.X) == 999999 || Math.Abs(vectorFromPoint.Y) == 999999) vectorFromPoint = new Point(0, 0); // TODO: Figure out what '999999' is supposed to do D:
                        fromAnchorPoint = new Point(anchorPoint.X - vectorFromPoint.X, anchorPoint.Y - vectorFromPoint.Y);
                    }
                }
                Point partOrigin = c.Item1.Origin ?? Point.Empty;

                return new Tuple<Frame, Point, float?>(
                    c.Item1,
                    new Point(fromAnchorPoint.X - partOrigin.X, fromAnchorPoint.Y - partOrigin.Y),
                    c.Item2
                );
            }).ToArray();

            return positionedFrames;
        }

        public byte[] Render(SpriteSheetFormat format, Func<Image<Rgba32>, byte[]> convertImg)
        {
            if ((format & SpriteSheetFormat.PDNZip) == SpriteSheetFormat.PDNZip)
                return RenderZipPDN(convertImg);
            else
                return convertImg(Render());
        }

        private byte[] RenderZipPDN(Func<Image<Rgba32>, byte[]> convertImg)
        {
            Tuple<Frame, Point, float?>[] positionedFrames = GetFrameParts();

            float minX = positionedFrames.Select(c => c.Item2.X).Min();
            float maxX = positionedFrames.Select(c => c.Item2.X + c.Item1.Image.Width).Max();
            float minY = positionedFrames.Select(c => c.Item2.Y).Min();
            float maxY = positionedFrames.Select(c => c.Item2.Y + c.Item1.Image.Height).Max();
            Size center = new Size((int)((maxX - minX) / 2), (int)((maxY - minY) / 2));
            Size offset = new Size((int)minX, (int)minY);

            Tuple<Frame, Point, float?> body = positionedFrames.Where(c => ((c.Item1.Position?.Equals("body") ?? false) || (c.Item1.Position?.Equals("backBody") ?? false)) && c.Item1.MapOffset.ContainsKey("neck") && c.Item1.MapOffset.ContainsKey("navel")).First();

            Tuple<Frame, Image<Rgba32>>[] parts = positionedFrames.Select((frame, index) =>
            {
                Image<Rgba32> destination = new Image<Rgba32>((int)((maxX - minX) + (Padding * 2)), (int)((maxY - minY) + (Padding * 2)));
                destination.Mutate(x =>
                {
                    Image<Rgba32> framePart = frame.Item1.Image;
                    if (frame.Item3.HasValue)
                    {
                        framePart = framePart.Clone();
                        framePart.Mutate(c => c.Hue(frame.Item3.Value));
                    }
                    x.DrawImage(
                        framePart,
                        1,
                        new Point(
                            (int)(frame.Item2.X - minX),
                            (int)(frame.Item2.Y - minY)
                        )
                    );
                });

                return new Tuple<Frame, Image<Rgba32>>(frame.Item1, Transform(destination, body, minX, minY, maxX, maxY, index == 0));
            }).ToArray();

            using (MemoryStream mem = new MemoryStream())
            {
                using (ZipArchive archive = new ZipArchive(mem, ZipArchiveMode.Create, true))
                {
                    ZipArchiveEntry PaintDotNet = archive.CreateEntry("PaintDotNet.txt", CompressionLevel.NoCompression);
                    using (Stream sig = PaintDotNet.Open())
                        sig.Write(Encoding.UTF8.GetBytes("PDN3"), 0, 4);

                    ConcurrentBag<Tuple<string, byte[]>> bag = new ConcurrentBag<Tuple<string, byte[]>>(parts.Select((c, i) => new Tuple<string, byte[]>($"L{i + 1},R1,C1,{c.Item1.Position},visible,normal,255.png", convertImg(c.Item2))));

                    while(bag.TryTake(out Tuple<string, byte[]> frameData))
                    {
                        ZipArchiveEntry entry = archive.CreateEntry(frameData.Item1, CompressionLevel.Optimal);
                        using (Stream entryData = entry.Open())
                        {
                            entryData.Write(frameData.Item2, 0, frameData.Item2.Length);
                            entryData.Flush();
                        }
                    }
                }

                return mem.ToArray();
            }
        }

        public Image<Rgba32> Render()
        {
            Tuple<Frame, Point, float?>[] positionedFrames = GetFrameParts();

            float minX = positionedFrames.Select(c => c.Item2.X).Min();
            float maxX = positionedFrames.Select(c => c.Item2.X + c.Item1.Image.Width).Max();
            float minY = positionedFrames.Select(c => c.Item2.Y).Min();
            float maxY = positionedFrames.Select(c => c.Item2.Y + c.Item1.Image.Height).Max();
            Size center = new Size((int)((maxX - minX) / 2), (int)((maxY - minY) / 2));
            Size offset = new Size((int)minX, (int)minY);

            Image<Rgba32> destination = new Image<Rgba32>((int)((maxX - minX) + (Padding * 2)), (int)((maxY - minY) + (Padding * 2)));
            destination.Mutate(x => positionedFrames.ForEach(frame => {
                Image<Rgba32> framePart = frame.Item1.Image;
                if (frame.Item3.HasValue)
                {
                    framePart = framePart.Clone();
                    framePart.Mutate(c => c.Hue(frame.Item3.Value));
                }
                x.DrawImage(
                    framePart,
                    1,
                    new Point(
                        (int)(frame.Item2.X - minX),
                        (int)(frame.Item2.Y - minY)
                    )
                );
            }));

            Tuple<Frame, Point, float?> body = positionedFrames.Where(c => ((c.Item1.Position?.Equals("body") ?? false) || (c.Item1.Position?.Equals("backBody") ?? false)) && c.Item1.MapOffset.ContainsKey("neck") && c.Item1.MapOffset.ContainsKey("navel")).First();

            return Transform(destination, body, minX, minY, maxX, maxY);
        }

        int lcmn(int[] numbers) => numbers.Aggregate(lcm);
        int lcm(int a, int b) => Math.Abs(a * b) / GCD(a, b);
        int GCD(int a, int b) => b == 0 ? a : GCD(b, a % b);

        public Tuple<Image<Rgba32>, Dictionary<string, Point>, Dictionary<string, int>, int> RenderWithDetails()
        {
            Dictionary<string, Point> offsets = new Dictionary<string, Point>();
            Tuple<Frame, Point, float?>[] positionedFrames = GetFrameParts(offsets);

            float minX = positionedFrames.Select(c => c.Item2.X).Min();
            float maxX = positionedFrames.Select(c => c.Item2.X + c.Item1.Image.Width).Max();
            float minY = positionedFrames.Select(c => c.Item2.Y).Min();
            float maxY = positionedFrames.Select(c => c.Item2.Y + c.Item1.Image.Height).Max();
            Size center = new Size((int)((maxX - minX) / 2), (int)((maxY - minY) / 2));
            Size offset = new Size((int)minX, (int)minY);

            Image<Rgba32> destination = new Image<Rgba32>((int)((maxX - minX) + (Padding * 2)), (int)((maxY - minY) + (Padding * 2)));
            destination.Mutate(x => positionedFrames.ForEach(frame => {
                Image<Rgba32> framePart = frame.Item1.Image;
                if (frame.Item3.HasValue)
                {
                    framePart = framePart.Clone();
                    framePart.Mutate(c => c.Hue(frame.Item3.Value));
                }
                x.DrawImage(
                    framePart,
                    1,
                    new Point(
                        (int)(frame.Item2.X - minX),
                        (int)(frame.Item2.Y - minY)
                    )
                );
            }));

            Tuple<Frame, Point, float?> body = positionedFrames.Where(c => ((c.Item1.Position?.Equals("body") ?? false) || (c.Item1.Position?.Equals("backBody") ?? false)) && c.Item1.MapOffset.ContainsKey("neck") && c.Item1.MapOffset.ContainsKey("navel")).First();

            Image<Rgba32> original = destination;
            Size originalSize = new Size(original.Width, original.Height);
            destination = Transform(destination, body, minX, minY, maxX, maxY);
            Size nameWidthAdjustment = new Size((int)NameWidthAdjustmentX, 0);
            offsets.Add("bodyOrigin", Point.Subtract(body.Item2, nameWidthAdjustment));
            offsets.Add("navelReal", Point.Subtract(new Point(-(int)minX, -(int)minY), nameWidthAdjustment));
            offsets.Add("centerReal", Point.Add(new Point(originalSize.Width / 2, originalSize.Height / 2), nameWidthAdjustment));
            offsets.Add("bodyCenterX", Point.Add(Point.Add(new Point((int)(body.Item2.X - minX), (int)(body.Item2.Y - minY)), new Size((int)(body.Item1.Image.Width / 2f), 0)), nameWidthAdjustment));
            offsets.Add("min", Point.Add(new Point((int)minX, (int)minY), nameWidthAdjustment));
            offsets.Add("max", Point.Add(new Point((int)maxX, (int)maxY), nameWidthAdjustment));

            if (Zoom != 1) offsets = offsets.ToDictionary(c => c.Key, c => new Point((int)(c.Value.X * Zoom), (int)(c.Value.Y * Zoom)));

            offsets.Add("feetCenter", calcFeetCenter(body, minX, minY, destination));

            int delay = 120;
            if (HasMount)
            {
                string requiredAnimation = AnimationName;
                if (AnimationName != "rope" && AnimationName != "ladder" && AnimationName != "sit") requiredAnimation = "sit";

                delay = FrameDelays[requiredAnimation][FrameNumber % FrameDelays[requiredAnimation].Length];
            }
            else if (!HasChair)
                delay = FrameDelays[AnimationName][FrameNumber % FrameDelays[AnimationName].Length];

            return new Tuple<Image<Rgba32>, Dictionary<string, Point>, Dictionary<string, int>, int>(
                destination,
                offsets,
                FrameCounts,
                delay
            );
        }

        Point calcFeetCenter(Tuple<Frame, Point, float?> body, float minX, float minY, Image<Rgba32> destination)
        {
            Point bodyOrigin = body.Item1.OriginOrZero;
            Point feetCenter = new Point(
                (int)(((body.Item2.X - minX) + bodyOrigin.X) - NameWidthAdjustmentX) - 4,
                (int)((body.Item2.Y - minY) + bodyOrigin.Y)
            );
            feetCenter = new Point((int)(feetCenter.X * Zoom), (int)(feetCenter.Y * Zoom));
            if (FlipX) feetCenter.X = destination.Width - feetCenter.X;
            return feetCenter;
        }

        Image<Rgba32> Transform(Image<Rgba32> destination, Tuple<Frame, Point, float?> body, float minX, float minY, float maxX, float maxY, bool includeName = true)
        {
            if (Mode == RenderMode.Compact)
            {
                Size bodyShouldBe = new Size(36, 55);
                Point cropOrigin = Point.Subtract(Point.Subtract(body.Item2, bodyShouldBe), new Size((int)minX, (int)minY));
                Rectangle cropArea = new Rectangle((int)Math.Max(cropOrigin.X, 0), (int)Math.Max(cropOrigin.Y, 0), 96, 96);
                Point cropOffsetFromOrigin = new Point(cropArea.X - cropOrigin.X, cropArea.Y - cropOrigin.Y);

                if (cropArea.Right > destination.Width) cropArea.Width = (int)(destination.Width - cropOrigin.X);
                if (cropArea.Bottom > destination.Height) cropArea.Height = (int)(destination.Height - cropOrigin.Y);

                Image<Rgba32> compact = new Image<Rgba32>(96, 96);
                destination.Mutate(c => c.Crop(cropArea));
                compact.Mutate(c => c.DrawImage(
                    destination,
                    1,
                    //new Size(cropArea.Width, cropArea.Height), // I *think* this will just be omitted anyways as it should basically be the same as compact size or cropArea size
                    new Point((int)cropOffsetFromOrigin.X, (int)cropOffsetFromOrigin.Y)
                ));
                destination.Dispose();

                return compact;
            }
            else if (Mode == RenderMode.Centered)
            {
                Size bodyCenter = Size.Add(new Size((int)(body.Item2.X - minX), (int)(body.Item2.Y - minY)), new Size((int)(body.Item1.Image.Width / 2f), 0));
                Point imageCenter = new Point(destination.Width / 2, destination.Height / 2);
                // Positive values = body is left/above, negative = body is right/below
                Point distanceFromCen = Point.Subtract(imageCenter, bodyCenter);
                Point distanceFromCenter = new Point(distanceFromCen.X * 2, distanceFromCen.Y * 2);
                Image<Rgba32> centered = new Image<Rgba32>(destination.Width + (int)Math.Abs(distanceFromCenter.X), destination.Height + (int)Math.Abs(distanceFromCenter.Y));
                centered.Mutate(c => c.DrawImage(destination, 1, new Point((int)Math.Max(distanceFromCenter.X, 0), (int)Math.Max(distanceFromCenter.Y, 0))));
                destination.Dispose();

                return centered;
            }
            else if (Mode == RenderMode.NavelCenter)
            {
                Point imageCenter = new Point(destination.Width / 2, destination.Height / 2);
                Point distanceFromCen = Point.Add(imageCenter, new Size((int)minX, (int)minY));
                Point distanceFromCenter = new Point(distanceFromCen.X * 2, distanceFromCen.Y * 2);
                Image<Rgba32> centered = new Image<Rgba32>(destination.Width + (int)Math.Abs(distanceFromCenter.X), destination.Height + (int)Math.Abs(distanceFromCenter.Y));
                centered.Mutate(c => c.DrawImage(destination, 1, new Point((int)Math.Max(distanceFromCenter.X, 0), (int)Math.Max(distanceFromCenter.Y, 0))));
                destination.Dispose();

                return centered;
            }
            else if (Mode == RenderMode.FeetCenter)
            {
                Size bodyCenter = Size.Add(new Size((int)(body.Item2.X - minX), (int)(body.Item2.Y - minY)), new Size((int)(body.Item1.Image.Width / 2f), body.Item1.Image.Height));
                Point imageCenter = new Point(destination.Width / 2, destination.Height / 2);
                Point distanceFromCen = Point.Subtract(imageCenter, bodyCenter);
                Point distanceFromCenter = new Point(distanceFromCen.X * 2, distanceFromCen.Y * 2);
                Image<Rgba32> centered = new Image<Rgba32>(destination.Width + (int)Math.Abs(distanceFromCenter.X), destination.Height + (int)Math.Abs(distanceFromCenter.Y));
                centered.Mutate(c => c.DrawImage(destination, 1, new Point((int)Math.Max(distanceFromCenter.X, 0), (int)Math.Max(distanceFromCenter.Y, 0))));
                destination.Dispose();

                return centered;
            }

            if (FlipX || Zoom != 1)
            {
                if (FlipX) destination.Mutate(x => { if (FlipX) x.Flip(FlipMode.Horizontal); });
                if (Zoom != 1 && Zoom != 0)
                {
                    if ((destination.Height * Zoom) < 50000 && (destination.Width * Zoom) < 50000)
                    {
                        destination.Mutate(c => c.Resize(new ResizeOptions()
                        {
                            Mode = ResizeMode.Stretch,
                            Sampler = new NearestNeighborResampler(),
                            Size = new Size((int)(destination.Width * Zoom), (int)(destination.Height * Zoom))
                        }));
                    }
                }
            }

            if (includeName && !string.IsNullOrEmpty(Name))
            {
                if (Name.Length > 64) Name = Name.Substring(0, 64);

                IEnumerable<Tuple<WZProperty, EquipSelection>> rings = equipped.Where(l => (l.Item2.ItemId / 1000) == 1112);
                Tuple<int?, WZProperty> labelRing = rings.Select(l =>
                {
                    return new Tuple<int?, WZProperty>(l.Item1.ResolveFor<int>("info/nameTag"), l.Item1);
                }).FirstOrDefault(l => l.Item1.HasValue);
                WZProperty nameTag = null;
                if (labelRing != null)
                    nameTag = labelRing.Item2.ResolveOutlink($"UI/NameTag/{labelRing.Item1}");

                Image<Rgba32> c = nameTag?.ResolveForOrNull<Image<Rgba32>>("c");
                Point cOrigin = nameTag?.ResolveFor<Point>("c/origin") ?? Point.Empty;
                Image<Rgba32> w = nameTag?.ResolveForOrNull<Image<Rgba32>>("w");
                Point wOrigin = nameTag?.ResolveFor<Point>("w/origin") ?? Point.Empty;
                Image<Rgba32> e = nameTag?.ResolveForOrNull<Image<Rgba32>>("e");
                Point eOrigin = nameTag?.ResolveFor<Point>("e/origin") ?? Point.Empty;
                int nameColorVal = nameTag?.ResolveFor<int>("clr") ?? -1;
                Rgba32 nameColor = new Rgba32();
                new Argb32((uint)nameColorVal).ToRgba32(ref nameColor);
                
                Point feetCenter = calcFeetCenter(body, minX, minY, destination);
                Font MaplestoryFont = fonts.Families
                    .First(f => f.Name.Equals("Arial Unicode MS", StringComparison.CurrentCultureIgnoreCase)).CreateFont(12, FontStyle.Regular);
                SizeF realNameSize = TextMeasurer.Measure(Name, new RendererOptions(MaplestoryFont));
                realNameSize = new SizeF((int)Math.Round(realNameSize.Width, MidpointRounding.AwayFromZero), (int)Math.Round(realNameSize.Height, MidpointRounding.AwayFromZero));
                int tagHeight = Math.Max(w?.Height ?? 0, e?.Height ?? 0);
                SizeF nameSize = SizeF.Add(realNameSize, new SizeF(10 + (w?.Width ?? 0) + (e?.Width ?? 0), 8 + (tagHeight > realNameSize.Height ? (tagHeight - realNameSize.Height) : 0)));
                SizeF halfSize = new SizeF(nameSize.Width / 2, nameSize.Height / 2);

                float nMinX = NameWidthAdjustmentX = (float)Math.Round(Math.Min(0, feetCenter.X - halfSize.Width));
                if (NameWidthAdjustmentX % 2 != 0) nMinX = NameWidthAdjustmentX = NameWidthAdjustmentX + 1;
                float nMaxX = Math.Max(destination.Width, feetCenter.X + halfSize.Width);
                Rectangle boxPosition = new Rectangle((int)((feetCenter.X - halfSize.Width) - nMinX) + 2, (int)feetCenter.Y + 4, (int)realNameSize.Width + (w?.Width ?? 0) + (e?.Width ?? 0), (int)realNameSize.Height);
                PointF textPosition = new PointF(boxPosition.X + 2 + (w?.Width ?? 0), (boxPosition.Y - 1) + (tagHeight > 0 ? tagHeight - 16 : 0) / 2);
                Image<Rgba32> withName = new Image<Rgba32>((int)Math.Max(nMaxX - nMinX, destination.Width), (int)Math.Max(feetCenter.Y + nameSize.Height, destination.Height + nameSize.Height));

                withName.Mutate(x =>
                {
                    if (nameTag == null)
                    {
                        boxPosition.Width = boxPosition.Width + 5;
                        x.Fill(new Rgba32(0, 0, 0, 128), boxPosition);
                        IPathCollection iPath = BuildCorners(boxPosition.X, boxPosition.Y, boxPosition.Width, boxPosition.Height, 4);
                        x.Fill(new Rgba32(0, 0, 0, 0), iPath);
                        x.DrawText(new TextGraphicsOptions() { VerticalAlignment = VerticalAlignment.Center }, Name, MaplestoryFont, nameColor, PointF.Add(textPosition, new PointF(0, realNameSize.Height / 2f + 1)));
                    }
                    else
                    {
                        x.DrawImage(w, 1, new Point((int)textPosition.X - wOrigin.X, (int)textPosition.Y - wOrigin.Y));
                        using (var cv = c.Clone(v => v.Resize(new Size((boxPosition.Width) - (w.Width + e.Width), c.Height))))
                            x.DrawImage(cv, 1, new Point((int)(textPosition.X) - cOrigin.X, (int)textPosition.Y - cOrigin.Y));
                        x.DrawImage(e, 1, new Point((int)(textPosition.X + boxPosition.Width - (w.Width + e.Width)), (int)textPosition.Y - eOrigin.Y));
                        x.DrawText(new TextGraphicsOptions() { VerticalAlignment = VerticalAlignment.Center }, Name, MaplestoryFont, nameColor, PointF.Add(textPosition, new PointF(0, realNameSize.Height / 2f - 1)));
                    }
                    x.DrawImage(destination, 1, new Point((int)Math.Round(-nMinX), 0));
                });
                destination.Dispose();

                return withName;
            }

            return destination;
        }

        IPathCollection BuildCorners(int x, int y, int width, int height, float cornerRadius)
        {
            // first create a square
            var rect = new RectangularPolygon(x-0.5f, y-0.5f, cornerRadius, cornerRadius);

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

        public void Preload()
        {
            if (this.preloaded) return;

            WZProperty character = wz.Resolve("Character");
            WZProperty itemEff = wz.Resolve("Effect/ItemEff");
            WZProperty installItem = wz.Resolve("Item/Install/0301");

            Equips.ForEach(c =>
            {
                if (c.ItemId >= 1902000 && c.ItemId <= 1993000)
                {
                    HasMount = true;
                }
                if ((c.ItemId / 10000) == 301)
                {
                    HasChair = true;
                    chairSitAction = wz.Resolve($"Item/Install/0301.img/{c.ItemId.ToString("D8")}/info/sitAction")?.ResolveForOrNull<string>() ?? "sit";
                }
            });

            string bodyId = SkinId.ToString("D8");
            string headId = (SkinId + 10000).ToString("D8");
            this.body = character.Resolve(bodyId);
            WZProperty head = character.Resolve(headId);

            List<EquipSelection> unResolved = new List<EquipSelection>();
            // Gather all of the equips (including body parts) and get their nodes
            IEnumerable<Tuple<WZProperty, EquipSelection>> equippedTmp = (new[]{
                new Tuple<WZProperty, EquipSelection>(body, new EquipSelection()),
                new Tuple<WZProperty, EquipSelection>(head, new EquipSelection())
            })
                .Concat(
                    Equips
                    .GroupBy(c => c.ItemId / 100)
                    .Select(c =>
                    {
                        int category = c.Key / 100;

                        Dictionary<string, EquipSelection> equipLookup = c.DistinctBy(b => b.ItemId).ToDictionary(b => b.ItemId.ToString("D8"), b => b);

                        IEnumerable<WZProperty> nodes = null;
                        if (category == 301)
                            nodes = wz.Resolve("Item/Install/0301").Children.Where(b => equipLookup.Keys.Contains(b.NameWithoutExtension));

                        if (category == 501)
                            nodes = wz.Resolve("Item/Cash/0501").Children.Where(b => equipLookup.Keys.Contains(b.NameWithoutExtension));

                        if (nodes == null)
                        {
                            if (!wz.categoryFolders.ContainsKey(c.Key))
                                nodes = new WZProperty[0];
                            else
                            {
                                string folder = wz.categoryFolders[c.Key];
                                WZProperty characterFolder = character.Resolve(folder).Resolve();
                                nodes = characterFolder.Children.Where(b => equipLookup.Keys.Contains(b.NameWithoutExtension));
                            }
                        }
                        else
                        {
                            nodes = nodes.Select(b =>
                            {
                                int? tamingMob = b.ResolveFor<int>("info/tamingMob");
                                if (tamingMob.HasValue && tamingMob.Value != 0)
                                {
                                    equipLookup[b.NameWithoutExtension].ItemId = tamingMob.Value;
                                    return character.Resolve($"TamingMob/{tamingMob.Value.ToString("D8")}");
                                }
                                else return b;
                            });
                        }

                        WZProperty[] nodeArr = nodes.ToArray();

                        if (nodeArr.Length != equipLookup.Count)
                        {
                            string[] nodeNames = nodeArr.Select(f => f.NameWithoutExtension).ToArray();
                            foreach (KeyValuePair<string, EquipSelection> equipSearch in equipLookup)
                                if (!nodeNames.Contains(equipSearch.Key))
                                    unResolved.Add(equipSearch.Value);
                        }

                        return nodes.Select(b => new Tuple<WZProperty, EquipSelection>(
                            b.Resolve(),
                            equipLookup.ContainsKey(b.NameWithoutExtension) ? equipLookup[b.NameWithoutExtension] : null
                        )).Where(b => b.Item2 != null);
                    }).Where(c => c != null).SelectMany(c => c)
                ).ToArray();

            if (unResolved.Count > 0)
            {
                Dictionary<string, EquipSelection> searchingFor = unResolved.ToDictionary(c => c.ItemId.ToString("D8"), c => c);
                List<Tuple<WZProperty, EquipSelection>> resolved = new List<Tuple<WZProperty, EquipSelection>>();

                IEnumerable<WZProperty> allEquips = character.Children.Select(b => b.Resolve()).SelectMany(b => b.Children);
                foreach (WZProperty equip in allEquips)
                {
                    if (searchingFor.ContainsKey(equip.NameWithoutExtension))
                    {
                        resolved.Add(new Tuple<WZProperty, EquipSelection>(equip, searchingFor[equip.NameWithoutExtension]));
                        searchingFor.Remove(equip.NameWithoutExtension);
                    }
                    if (searchingFor.Count == 0) break;
                }

                if (resolved.Count > 0) equippedTmp = equippedTmp.Concat(resolved);
                //if (resolved.Count != searchingFor.Count)
                //{
                //    // TODO: Log this
                //}
            }

            equipped = equippedTmp.Where(c => c != null && c.Item1 != null).ToArray();
            
            // Calculate the frame counts for all individual actions
            string[] actions = GetActions();
            FrameDelays = new Dictionary<string, int[]>();
            var a = equipped.Select(c =>
            {
                WZProperty itemNode = c.Item1;
                WZProperty node = itemNode; // Resolve all items and body parts to their correct nodes for the animation
                if (node.Children.Where(n => n.NameWithoutExtension != "info").All(n => int.TryParse(n.NameWithoutExtension, out int blah)))
                    node = node.Resolve($"{weaponType.ToString()}"); // If their selected animation doesn't exist, try ours, and then go to default as a fail-safe

                if (node == null) return null;
                if (node.Type == PropertyType.Lua) node = node.Resolve();

                Dictionary<string, WZProperty> children = node.Children.ToDictionary(b => b.NameWithoutExtension, b => b);

                return actions.Select(action =>
                {
                    WZProperty animationNode = null;
                    bool hasRequiredStance = HasChair || HasMount, isNotMount = c.Item2.ItemId < 1902000 || c.Item2.ItemId > 1993000;
                    string animationNameOrAction = c.Item2.AnimationName ?? action;
                    string forcedStance = HasChair ? chairSitAction : (HasMount && (AnimationName != "rope" && AnimationName != "ladder" && AnimationName != "sit") ? "sit" : AnimationName);

                    if (children.TryGetValue((hasRequiredStance && isNotMount) ? forcedStance : action, out animationNode)) { }
                    else if (children.TryGetValue(hasRequiredStance ? animationNameOrAction : "default", out animationNode)) { }
                    else if (children.TryGetValue(animationNameOrAction, out animationNode)) { }
                    else if (children.TryGetValue("default", out animationNode))
                    {
                        if (animationNode == null) animationNode = node.Resolve();
                    } else return null;

                    if (animationNode == null && !(c.Item2.ItemId >= 1902000 && c.Item2.ItemId <= 1993000 && (animationNode = node.Resolve("sit")) != null))
                            return null;

                    // Resolve to animation's frame
                    int frameCount = animationNode.Children.Where(k => int.TryParse(k.NameWithoutExtension, out int blah)).Select(k => int.Parse(k.NameWithoutExtension)).DefaultIfEmpty(-1).Max() + 1;

                    if (c.Item1 == body && frameCount > 0)
                    {
                        int frameForEntry = (c.Item2.EquipFrame ?? FrameNumber) % frameCount;
                        // Resolve for frame, and then ensure the frame is resolved completely. If there is no frame, then the animationNode likely contains the parts
                        WZProperty frameNode = animationNode.Resolve(frameForEntry.ToString())?.Resolve() ?? (frameCount == 1 ? animationNode.Resolve() : null);

                        int frameNodeDelay = frameNode.ResolveFor<int>("delay") ?? 0;
                        if (!FrameDelays.ContainsKey(animationNode.Name)) FrameDelays.Add(animationNode.Name, new int[frameCount]);
                        FrameDelays[animationNode.Name][frameForEntry] = frameNodeDelay;
                    }
                    return new Tuple<string, int, int>(action, c.Item2.ItemId, frameCount);
                }).Where(b => b != null).ToArray();
            }).Where(c => c != null).ToArray();
            var z = equipped.Select(c =>
            {
                IEnumerable<WZProperty> nodes = new WZProperty[] { itemEff.Resolve($"{c.Item2.ItemId}/effect") }; // Resolve the selected animation
                if (nodes.First() == null && (c.Item2.ItemId / 10000) == 301) nodes = installItem.Resolve($"{c.Item2.ItemId.ToString("D8")}").Children.Where(eff => eff.NameWithoutExtension.StartsWith("effect", StringComparison.CurrentCultureIgnoreCase));

                return nodes?.Where(node => node != null).Select(node =>
                {
                    return actions.Select(action =>
                    {

                        WZProperty effectNode = node.Resolve(c.Item2.AnimationName ?? action) ?? node.Resolve("default") ?? (node.Children.Any(b => b.NameWithoutExtension.Equals("0")) ? node : null);
                        if (effectNode == null) return null;
                        int frameCount = effectNode.Children.Where(k => int.TryParse(k.NameWithoutExtension, out int blah)).Select(k => int.Parse(k.NameWithoutExtension)).Max() + 1;
                        return new Tuple<string, int, int>(action, c.Item2.ItemId, frameCount);
                    });
                }).ToArray();
            }).Where(c => c != null).SelectMany(c => c).Where(b => b != null).ToArray();
            var y = a.SelectMany(c => c).ToArray();
            FrameCounts = y.GroupBy(c => c.Item1)
                .Select(c => {
                    int[] itemsFrameCounts = c.GroupBy(b => b.Item2).Where(b => b.Count() > 0).Select(b =>
                    {
                        int[] itemsActionFrameCounts = b.Where(d => d.Item3 > 0).Select(d => d.Item3).ToArray();
                        if (itemsActionFrameCounts.Length > 0)
                            return lcmn(itemsActionFrameCounts);
                        else return 0;
                    }).Where(b => b != 0).ToArray();
                    if (itemsFrameCounts.Length == 0) return null;
                    return new Tuple<string, int>(c.Key, lcmn(itemsFrameCounts));
                })
                .Where(c => c != null && c.Item2 != 0)
                .ToDictionary(c => c.Item1, c => c.Item2);

            // Get a cached version of the zmap
            zmap = (wz.Resolve("Base/zmap") ?? wz.Resolve("zmap")).Children.Select(b => b.NameWithoutExtension).Reverse().ToList();

            // Build a sorted list of defined exclusive locks from items
            IEnumerable<Tuple<int, string[], string[]>> exclusiveLockItems = equipped
                .OrderBy(c => zmap.IndexOf(c.Item1.ResolveForOrNull<string>("info/islot")?.Substring(0, 2)) * ((c.Item1.ResolveFor<bool>("info/cash") ?? false) ? 2 : 1))
                .Select(c => {
                    string islot = c.Item1.ResolveForOrNull<string>("info/islot") ?? "";
                    string vslot = c.Item1.ResolveForOrNull<string>("info/vslot") ?? "";
                    if ((int)(c.Item2.ItemId / 10000) == 104 && islot.Equals("MaPn")) islot = "Ma"; // No clue why normal shirts would claim to be overalls, but fuck off.
                    if ((int)(c.Item2.ItemId / 10000) == 104 && vslot.Equals("MaPn")) vslot = "Ma"; // No clue why normal shirts would claim to be overalls, but fuck off.
                    return new Tuple<int, string, string>(
                        c.Item2.ItemId,
                        vslot,
                        islot
                    );
                }) // Override item specific vslots here
                .Select(c => new Tuple<int, string[], string[]>(
                    c.Item1,
                    Enumerable.Range(0, c.Item2.Length / 2).Select((b, i) => c.Item2.Substring(i * 2, 2)).ToArray(),
                    Enumerable.Range(0, c.Item3.Length / 2).Select((b, i) => c.Item3.Substring(i * 2, 2)).ToArray()
                ));

            // Establish slots of equips
            Dictionary<string, int> exclusiveSlots = new Dictionary<string, int>();
            foreach (Tuple<int, string[], string[]> exclusiveLock in exclusiveLockItems)
                foreach (string locking in exclusiveLock.Item3)
                    if (exclusiveSlots.ContainsKey(locking))
                        exclusiveSlots[locking] = exclusiveLock.Item1;
                    else
                        exclusiveSlots.Add(locking, exclusiveLock.Item1);

            // Filter out equips that don't have locks on slots
            IEnumerable<Tuple<WZProperty, EquipSelection>> newEquipped = equipped;
            foreach (Tuple<int, string[], string[]> exclusiveLock in exclusiveLockItems)
            {
                bool locksAll = true;
                foreach (string locking in exclusiveLock.Item3)
                    locksAll &= exclusiveSlots.ContainsKey(locking) && exclusiveSlots[locking] == exclusiveLock.Item1;

                if (!locksAll)
                {
                    foreach (string locking in exclusiveLock.Item3)
                        if (exclusiveSlots[locking] == exclusiveLock.Item1)
                            exclusiveSlots.Remove(locking);
                    newEquipped = newEquipped.Where(c => c.Item2.ItemId != exclusiveLock.Item1);
                }
            }
            equipped = newEquipped.ToArray();

            // Build a dictionary between what is locked and what is locking it
            exclusiveLocks = new Dictionary<string, int>();
            foreach (Tuple<int, string[], string[]> exclusiveLock in exclusiveLockItems)
                if (exclusiveSlots.Any(slot => slot.Value == exclusiveLock.Item1))
                    foreach (string locking in exclusiveLock.Item2)
                        if (exclusiveLocks.ContainsKey(locking))
                            exclusiveLocks[locking] = exclusiveLock.Item1;
                        else
                            exclusiveLocks.Add(locking, exclusiveLock.Item1);

            // Build an smap dictionary to look up between what a position will require to lock before it can be rendered
            smap = (wz.Resolve("Base/smap") ?? wz.Resolve("smap")).Children
                .Where(c => c.ResolveForOrNull<string>() != null)
                .ToDictionary(c => c.NameWithoutExtension, c => (c.ResolveForOrNull<string>() ?? "").Replace("PnSo", "Pn"));

            // We need the weapon entry so we know what kind of weapon the character has equipped
            // Certain items require the weapon type to determine what kind of animation will be displayed
            Tuple<WZProperty, EquipSelection> weaponEntry = equipped.FirstOrDefault(c => c.Item1.Parent.NameWithoutExtension.Equals("Weapon"));
            // Default to weapon type `30`
            weaponType = weaponEntry?.Item1 != null && weaponEntry?.Item2 != null ? (int)((weaponEntry.Item2.ItemId - 1000000) / 10000d) : 30;
            // WeaponTypes of 70 are cash items, go back to 30.
            if (weaponType == 70) weaponType = 30;

            this.preloaded = true;
        }

        public string[] GetActions()
        {
            IEnumerable<IGrouping<int, int>> itemEntriesStr = Equips.Select(c => c.ItemId).Concat(new int[] { 1060002, 1040002 }).Where(c => c >= 30000).GroupBy(c => c / 100);

            WZProperty character = wz.Resolve("Character");
            IEnumerable<WZProperty> itemNodes = itemEntriesStr.Select(c =>
            {
                string[] names = c.Select(b => b.ToString("D8")).ToArray();
                if (!wz.categoryFolders.ContainsKey(c.Key)) return null;
                string folder = wz.categoryFolders[c.Key];
                WZProperty characterFolder = character.Resolve(folder);
                return characterFolder.Children.Where(b => names.Contains(b.NameWithoutExtension));
            }).Where(c => c != null).SelectMany(c => c).Select(c => c.Resolve()).ToArray();

            string[] firstItemAnimations = itemNodes.Where(c => c.NameWithoutExtension.Equals("01040002")).First().Children.Select(c => c.NameWithoutExtension).ToArray();

            return itemNodes.Skip(1)
                .SelectMany(c => c.Children.Where(b => firstItemAnimations.Contains(b.NameWithoutExtension)))
                .Select(c => c.NameWithoutExtension)
                .Distinct()
                .ToArray();
        }

        public IEnumerable<RankedFrame> GetAnimationParts(List<KeyValuePair<string, Point>[]> offsets)
        {
            Preload();

            bool hasFace = (body.Resolve(AnimationName) ?? body.Resolve("default")).ResolveFor<bool>($"{FrameNumber}/face") ?? true;

            Dictionary<string, int> exclusiveLocksRender = new Dictionary<string, int>(exclusiveLocks);

            WZProperty itemEff = wz.Resolve("Effect/ItemEff");
            WZProperty installItem = wz.Resolve("Item/Install/0301");

            // Resolve to action nodes and then to frame nodes
            IEnumerable<Tuple<WZProperty, EquipSelection>> frameParts = equipped.Select(c =>
            {
                WZProperty itemNode = c.Item1;
                WZProperty node = itemNode; // Resolve all items and body parts to their correct nodes for the animation
                if (node.Children.Where(n => n.NameWithoutExtension != "info").All(n => int.TryParse(n.NameWithoutExtension, out int blah)))
                    node = node.Resolve($"{weaponType.ToString()}"); // If their selected animation doesn't exist, try ours, and then go to default as a fail-safe

                if (node == null) return null;

                string requiredAnimation = AnimationName;
                if (HasMount && (AnimationName != "rope" && AnimationName != "ladder" && AnimationName != "sit")) requiredAnimation = "sit";
                if (HasChair) requiredAnimation = chairSitAction;

                WZProperty animationNode = node.Resolve((requiredAnimation != null && (c.Item2.ItemId < 1902000 || c.Item2.ItemId > 1993000)) ? requiredAnimation : (c.Item2.AnimationName ?? AnimationName)) ??
                    (requiredAnimation != null ? node.Resolve(c.Item2.AnimationName ?? AnimationName) : node.Resolve("default")) ?? node.Resolve("default");
                if (animationNode == null)
                {
                    if (!(c.Item2.ItemId >= 1902000 && c.Item2.ItemId <= 1993000 && (animationNode = node.Resolve(requiredAnimation ?? "sit")) != null))
                        return null;
                }
                // Resolve to animation's frame
                int frameCount = animationNode.Children.Where(k => int.TryParse(k.NameWithoutExtension, out int blah)).Select(k => int.Parse(k.NameWithoutExtension)).DefaultIfEmpty(0).Max() + 1;
                int frameForEntry = (c.Item2.EquipFrame ?? FrameNumber) % frameCount;
                // Resolve for frame, and then ensure the frame is resolved completely. If there is no frame, then the animationNode likely contains the parts
                WZProperty frameNode = animationNode.Resolve(frameForEntry.ToString())?.Resolve() ?? (frameCount == 1 ? animationNode.Resolve() : null);
                if (frameNode == null) return null;
                // Resolve to only children parts that have appropriate locks
                return frameNode.Children.Where(framePart =>
                {
                    // Ensure we're only getting the parts, not the meta attributes that are in the frames
                    WZProperty framePartNode = framePart.Resolve();
                    if (framePartNode == null || framePartNode.Type != PropertyType.Canvas) return false;

                    offsets.Add(framePartNode.Resolve("map")?.Children.Select(mapOffset => new KeyValuePair<string, Point>(mapOffset.NameWithoutExtension, mapOffset.ResolveFor<Point>() ?? Point.Empty)).ToArray());

                    if (!ElfEars && framePart.NameWithoutExtension.Equals("ear", StringComparison.CurrentCultureIgnoreCase)) return false;
                    if (!LefEars && framePart.NameWithoutExtension.Equals("lefEar", StringComparison.CurrentCultureIgnoreCase)) return false;
                    if (framePart.NameWithoutExtension.Equals("highlefEar", StringComparison.CurrentCultureIgnoreCase)) return false;

                    // If the z-position is equal to the equipCategory, the required locks are the vslot
                    // This seems to resolve the caps only requiring the locks of vslot, not the full `cap` in smap
                    string equipCategory = framePartNode.Path.Split(System.IO.Path.DirectorySeparatorChar)[1];
                    string zPosition = framePartNode.Resolve().ResolveForOrNull<string>("z") ?? framePartNode.ResolveForOrNull<string>("../z") ?? framePartNode.NameWithoutExtension;
                    bool sameZAsContainer = !zPosition.Equals(equipCategory, StringComparison.CurrentCultureIgnoreCase);

                    string requiredLockFull = smap.ContainsKey(framePart.NameWithoutExtension) && !sameZAsContainer ? smap[framePart.NameWithoutExtension] : itemNode.ResolveForOrNull<string>("info/vslot") ?? "";
                    string[] requiredLocks = Enumerable.Range(0, requiredLockFull.Length / 2).Select(k => requiredLockFull.Substring(k * 2, 2)).ToArray();
                    // Determine if we have locks
                    bool hasLocks = requiredLocks.Count() == 0 || requiredLocks.All(requiredLock => !exclusiveLocks.ContainsKey(requiredLock) || exclusiveLocks[requiredLock] == c.Item2.ItemId);
                    // If we have the lock, we need to ensure we retain the lock to prevent other items from getting the lock

                    // If we don't have the lock and we're assuming we're using the parent's vslot, try using the smap.
                    // This seems to resolve the `hair` z using the more exclusive vslot
                    if (sameZAsContainer && !hasLocks)
                    {
                        requiredLockFull = smap.ContainsKey(framePart.NameWithoutExtension) ? smap[framePart.NameWithoutExtension] : itemNode.ResolveForOrNull<string>("info/vslot");
                        if ((int)(c.Item2.ItemId / 10000) == 104 && requiredLockFull.Equals("MaPn")) requiredLockFull = "Ma";
                        requiredLocks = Enumerable.Range(0, requiredLockFull.Length / 2).Select(k => requiredLockFull.Substring(k * 2, 2)).ToArray();
                        // Determine if we have locks
                        hasLocks = requiredLocks.All(requiredLock => !exclusiveLocks.ContainsKey(requiredLock) || exclusiveLocks[requiredLock] == c.Item2.ItemId);
                    }

                    if (hasLocks)
                        foreach (string requiredLock in requiredLocks)
                            if (!exclusiveLocks.ContainsKey(requiredLock))
                                exclusiveLocks.Add(requiredLock, c.Item2.ItemId);
                    return hasLocks;
                }).Select(o => new Tuple<WZProperty, EquipSelection>(o, c.Item2)).ToArray();
            })
            .Where(c => c != null)
            .SelectMany(c => c)
            .Concat(Equips.Select(c =>
            { // Concat any effects for items equipped
                IEnumerable<WZProperty> nodes = new WZProperty[] { itemEff.Resolve($"{c.ItemId}/effect") }; // Resolve the selected animation
                if (nodes.First() == null && (c.ItemId / 10000) == 301) nodes = installItem.Resolve($"{c.ItemId.ToString("D8")}").Children.Where(eff => eff.NameWithoutExtension.StartsWith("effect", StringComparison.CurrentCultureIgnoreCase));

                return nodes?.Where(node => node != null).Select(node =>
                {
                    WZProperty effectNode = node.Resolve(c.AnimationName ?? AnimationName) ?? node.Resolve("default") ?? (node.Children.Any(b => b.NameWithoutExtension.Equals("0")) ? node : null);
                    if (effectNode == null) return null;
                    int frameCount = effectNode.Children.Where(k => int.TryParse(k.NameWithoutExtension, out int blah)).Select(k => int.Parse(k.NameWithoutExtension)).Max() + 1;
                    int frameForEntry = (c.EquipFrame ?? FrameNumber) % frameCount;
                    return new Tuple<WZProperty, EquipSelection>(effectNode.Resolve(frameForEntry.ToString())?.Resolve(), c);
                });
            }).Where(nodes => nodes != null).SelectMany(eff => eff.Where(node => node != null)))
            .Where(c => c != null);

            ConcurrentBag<RankedFrame> rankedFrames = new ConcurrentBag<RankedFrame>();

            while (!Parallel.ForEach(frameParts ?? new Tuple<WZProperty, EquipSelection>[0], (c) =>
            {
                string zIndex = c.Item1.ResolveForOrNull<string>("../z") ?? c.Item1.Resolve().ResolveForOrNull<string>("z") ?? "0";
                int zPosition = 0;
                if (!int.TryParse(zIndex, out zPosition))
                    zPosition = zmap.IndexOf(zIndex);
                else zPosition = (zPosition - 1) * 500;

                if (!hasFace && zIndex.EndsWith("BelowFace", StringComparison.CurrentCultureIgnoreCase)) zPosition -= 100;

                RankedFrame ranked = new RankedFrame(Frame.Parse(c.Item1), zPosition, c.Item2);

                if (ranked?.frame?.Position == "face" && !hasFace) return;

                rankedFrames.Add(ranked);
            }).IsCompleted) Thread.Sleep(1);

            RankedFrame[] rankedFramesArray = new RankedFrame[rankedFrames.Count];
            int i = 0;
            while (rankedFrames.TryTake(out RankedFrame rankedFrame)) rankedFramesArray[i++] = rankedFrame;

            return rankedFramesArray;
        }
    }

    public class EquipSelection
    {
        public int ItemId;
        public string AnimationName;
        public int? EquipFrame;
        public float? Hue;
    }

    public class RankedFrame
    {
        public readonly Frame frame;
        public readonly int ranking;
        public EquipSelection underlyingEquip;

        public RankedFrame(Frame frame, int ranking, EquipSelection underlyingEquip)
        {
            this.frame = frame;
            this.ranking = ranking;
            this.underlyingEquip = underlyingEquip;
        }
    }

    public class PositionedFrame
    {
        public readonly Frame frame;
        public readonly Point position;

        public PositionedFrame(Frame frame, Point position)
        {
            this.frame = frame;
            this.position = position;
        }
    }

    public enum RenderMode
    {
        Full,
        Compact,
        Centered,
        NavelCenter,
        FeetCenter
    }

    public enum SpriteSheetFormat
    {
        Plain = 0,
        PDNZip = 1,
        Minimal = 2
    }
}