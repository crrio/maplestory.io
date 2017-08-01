using PKG1;
using System;
using ImageSharp;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using MoreLinq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using SixLabors.Primitives;

namespace WZData.MapleStory.Characters {
    public class CharacterAvatar {
        public int SkinId;
        public EquipSelection[] Equips;
        public RenderMode Mode;
        public int FrameNumber;
        public string AnimationName;
        private readonly PackageCollection wz;
        public int Padding;
        public bool ElfEars;
        private int weaponType;
        private Dictionary<string, string> smap;
        private Dictionary<string, int> exclusiveLocks;
        private List<string> zmap;
        public Tuple<WZProperty, EquipSelection>[] equipped;
        private bool preloaded;
        private WZProperty body;

        public CharacterAvatar(PackageCollection wz) {
            this.wz = wz;
        }
        public CharacterAvatar(CharacterAvatar old) {
            this.SkinId = old.SkinId;
            this.Equips = old.Equips;
            this.Mode = old.Mode;
            this.FrameNumber = old.FrameNumber;
            this.AnimationName = old.AnimationName;
            this.wz = old.wz;
            this.Padding = old.Padding;
            this.ElfEars = old.ElfEars;
            this.weaponType = old.weaponType;
            this.smap = old.smap;
            this.exclusiveLocks = old.exclusiveLocks;
            this.zmap = old.zmap;
            this.equipped = old.equipped;
            this.preloaded = old.preloaded;
        }

        public Image<Rgba32> Render() {
            RankedFrame[] partsData = GetAnimationParts().OrderBy(c => c.ranking).ToArray();
            Frame[] partsFrames = partsData.Select(c => c.frame).ToArray();

            Dictionary<string, Point> anchorPositions = new Dictionary<string, Point>() { { "navel", new Point(0, 0) } };
            RankedFrame bodyFrame = partsData.FirstOrDefault(c => c.frame.Position == "body" || c.frame.Position == "backBody");
            Point neckOffsetBody = bodyFrame.frame.MapOffset["neck"];
            Point navelOffsetBody = bodyFrame.frame.MapOffset["navel"];

            List<KeyValuePair<string, Point>[]> offsets = partsFrames
                .Where(c => c.MapOffset != null)
                .Select(c => c.MapOffset.Where(k => !k.Key.Equals("zero")).ToArray())
                .Where(c => c.Length > 0)
                .ToList();
            while (offsets.Count > 0) {
                KeyValuePair<string, Point>[] offsetPairing = offsets.FirstOrDefault(c => c.Any(b => anchorPositions.ContainsKey(b.Key)));
                if (offsetPairing == null) break;
                KeyValuePair<string, Point> anchorPointEntry = offsetPairing.Where(c => anchorPositions.ContainsKey(c.Key)).FirstOrDefault();
                // Handle alert position? How to :<
                Point anchorPoint = anchorPoint = anchorPositions[anchorPointEntry.Key];
                Point vectorFromPoint = anchorPointEntry.Value;
                Point fromAnchorPoint = new Point(anchorPoint.X - vectorFromPoint.X, anchorPoint.Y - vectorFromPoint.Y);

                foreach (KeyValuePair<string, Point> childAnchorPoint in offsetPairing.Where(c => c.Key != anchorPointEntry.Key))
                    if (!anchorPositions.ContainsKey(childAnchorPoint.Key))
                        anchorPositions.Add(childAnchorPoint.Key, new Point(fromAnchorPoint.X + childAnchorPoint.Value.X, fromAnchorPoint.Y + childAnchorPoint.Value.Y));

                offsets.Remove(offsetPairing);
            }

            Tuple<Frame, Point>[] positionedFrames = partsFrames.Select(c => {
                // Some effects are centered off of the neck
                Point fromAnchorPoint = neckOffsetBody;
                if (c.MapOffset != null) {
                    // Some effects are centered on the origin (0,0)
                    if (c.MapOffset.All(b => b.Key.Equals("zero"))) {
                        fromAnchorPoint = new Point(-navelOffsetBody.X, -navelOffsetBody.Y);
                    } else { // Default positioning based off of offsets
                        KeyValuePair<string, Point> anchorPointEntry = (c.MapOffset ?? new Dictionary<string, Point>()).Where(b => anchorPositions.ContainsKey(b.Key)).DefaultIfEmpty(new KeyValuePair<string, Point>(null, Point.Empty)).First();
                        Point anchorPoint = anchorPoint = anchorPositions[anchorPointEntry.Key];
                        Point vectorFromPoint = anchorPointEntry.Value;
                        fromAnchorPoint = new Point(anchorPoint.X - vectorFromPoint.X, anchorPoint.Y - vectorFromPoint.Y);
                    }
                }
                Point partOrigin = c.Origin ?? Point.Empty;

                return new Tuple<Frame, Point>(
                    c,
                    new Point(fromAnchorPoint.X - partOrigin.X, fromAnchorPoint.Y - partOrigin.Y)
                );
            }).ToArray();

            float minX = positionedFrames.Select(c => c.Item2.X).Min();
            float maxX = positionedFrames.Select(c => c.Item2.X + c.Item1.Image.Width).Max();
            float minY = positionedFrames.Select(c => c.Item2.Y).Min();
            float maxY = positionedFrames.Select(c => c.Item2.Y + c.Item1.Image.Height).Max();
            Size center = new Size((int)((maxX - minX) / 2), (int)((maxY - minY) / 2));
            Size offset = new Size((int)minX, (int)minY);

            Image<Rgba32> destination = new Image<Rgba32>((int)((maxX - minX) + (Padding * 2)), (int)((maxY - minY) + (Padding * 2)));
            foreach (Tuple<Frame, Point> frame in positionedFrames)
                destination.DrawImage(
                    frame.Item1.Image,
                    1,
                    new Size(
                        frame.Item1.Image.Width,
                        frame.Item1.Image.Height
                    ),
                    new Point(
                        (int)(frame.Item2.X - minX),
                        (int)(frame.Item2.Y - minY)
                    )
                );

            Tuple<Frame, Point> body = positionedFrames.Where(c => c.Item1.Position.Equals("body") || c.Item1.Position.Equals("backBody")).First();

            if (Mode == RenderMode.Compact)
            {
                Size bodyShouldBe = new Size(36, 55);
                Point cropOrigin = Point.Subtract(body.Item2, bodyShouldBe);
                Rectangle cropArea = new Rectangle((int)Math.Max(cropOrigin.X, 0), (int)Math.Max(cropOrigin.Y, 0), 96, 96);
                Point cropOffsetFromOrigin = new Point(cropArea.X - cropOrigin.X, cropArea.Y - cropOrigin.Y);

                if (cropArea.Right > destination.Width) cropArea.Width = (int)(destination.Width - cropOrigin.X);
                if (cropArea.Bottom > destination.Height) cropArea.Height = (int)(destination.Height - cropOrigin.Y);

                Image<Rgba32> compact = new Image<Rgba32>(96, 96);
                compact.DrawImage(
                    destination.Crop(cropArea),
                    1,
                    new Size(cropArea.Width, cropArea.Height),
                    new Point((int)cropOffsetFromOrigin.X, (int)cropOffsetFromOrigin.Y)
                );

                return compact;
            } else if (Mode == RenderMode.Centered)
            {
                Size bodyCenter = Size.Add(new Size((int)(body.Item2.X - minX), (int)(body.Item2.Y - minY)), new Size((int)(body.Item1.Image.Width / 2f), 0));
                Point imageCenter = new Point(destination.Width / 2, destination.Height / 2);
                // Positive values = body is left/above, negative = body is right/below
                Point distanceFromCen = Point.Subtract(imageCenter, bodyCenter);
                Point distanceFromCenter = new Point(distanceFromCen.X * 2, distanceFromCen.Y * 2);
                Image<Rgba32> centered = new Image<Rgba32>(destination.Width + (int)Math.Abs(distanceFromCenter.X), destination.Height + (int)Math.Abs(distanceFromCenter.Y));
                centered.DrawImage(destination, 1, new Size(destination.Width, destination.Height), new Point((int)Math.Max(distanceFromCenter.X, 0), (int)Math.Max(distanceFromCenter.Y, 0)));

                return centered;
            }

            return destination;
        }

        public void Preload() {
            if (this.preloaded) return;

            string bodyId = SkinId.ToString("D8");
            string headId = (SkinId + 10000).ToString("D8");
            this.body = wz.Resolve($"Character/{bodyId}");
            WZProperty head = wz.Resolve($"Character/{headId}");

            // Cache the node points for all equips, should be relatively quick as it's only node names and IDs
            IEnumerable<WZProperty> item = wz.Resolve("Character/").Children.Values
                .Where(c => c.Type != PropertyType.Image)
                .SelectMany(c => c.Children.Values)
                .ToArray();

            // Gather all of the equips (including body parts) and get their nodes
            equipped = (new []{
                new Tuple<WZProperty, EquipSelection>(body, new EquipSelection()),
                new Tuple<WZProperty, EquipSelection>(head, new EquipSelection())
            })
                .Concat(
                    Equips
                        .Select(c => new Tuple<WZProperty, EquipSelection>(
                            item.FirstOrDefault(i => i.Name.Equals($"{c.ItemId.ToString("D8")}")),
                            c
                        ))
                )
                .Where(c => c != null && c.Item1 != null)
                .ToArray();

            // Get a cached version of the zmap
            zmap = wz.Resolve("Base/zmap").Children.Keys.Reverse().ToList();

            // Build a sorted list of defined exclusive locks from items
            IEnumerable<Tuple<int, string[]>> exclusiveLockItems = equipped
                .OrderBy(c =>  zmap.IndexOf(c.Item1.ResolveForOrNull<string>("info/islot")?.Substring(0, 2)))
                .Select(c => new Tuple<int, string>(c.Item2.ItemId, c.Item1.ResolveForOrNull<string>("info/vslot") ?? "")) // Override item specific vslots here
                .Select(c => new Tuple<int, string[]>(c.Item1, Enumerable.Range(0, c.Item2.Length / 2).Select((b, i) => c.Item2.Substring(i * 2, 2)).ToArray()));

            // Build a dictionary between what is locked and what is locking it
            exclusiveLocks = new Dictionary<string, int>();
            foreach(Tuple<int, string[]> exclusiveLock in exclusiveLockItems)
                foreach(string locking in exclusiveLock.Item2)
                    if (exclusiveLocks.ContainsKey(locking))
                        exclusiveLocks[locking] = exclusiveLock.Item1;
                    else
                        exclusiveLocks.Add(locking, exclusiveLock.Item1);

            // Build an smap dictionary to look up between what a position will require to lock before it can be rendered
            smap = wz.Resolve("Base/smap").Children
                .Where(c => c.Value.ResolveForOrNull<string>() != null)
                .ToDictionary(c => c.Key, c => c.Value.ResolveForOrNull<string>() ?? "");

            // We need the weapon entry so we know what kind of weapon the character has equipped
            // Certain items require the weapon type to determine what kind of animation will be displayed
            Tuple<WZProperty, EquipSelection> weaponEntry = equipped.FirstOrDefault(c => c.Item1.Parent.Name.Equals("Weapon"));
            // Default to weapon type `30`
            weaponType = weaponEntry?.Item1 != null && weaponEntry?.Item2 != null ? (int)((weaponEntry.Item2.ItemId  - 1000000) / 10000d) : 30;
            // WeaponTypes of 70 are cash items, go back to 30.
            if (weaponType == 70) weaponType = 30;

            this.preloaded = true;
        }

        public IEnumerable<RankedFrame> GetAnimationParts() {
            Preload();

            bool hasFace = (body.Resolve(AnimationName) ?? body.Resolve("default")).ResolveFor<bool>($"{FrameNumber}/face") ?? true;

            Dictionary<string, int> exclusiveLocksRender = new Dictionary<string, int>(exclusiveLocks);
            // Resolve to action nodes and then to frame nodes
            IEnumerable<WZProperty> frameParts = equipped.Select(c => {
                WZProperty itemNode = c.Item1;
                WZProperty node = itemNode; // Resolve all items and body parts to their correct nodes for the animation
                if (node.Children.Keys.Where(name => name != "info").All(name => int.TryParse(name, out int blah)))
                    node = node.Resolve($"{weaponType.ToString()}"); // If their selected animation doesn't exist, try ours, and then go to default as a fail-safe

                if (node == null) return null;

                WZProperty animationNode = node.Resolve(c.Item2.AnimationName ?? AnimationName) ?? node.Resolve("default");

                if (animationNode == null) return null;
                // Resolve to animation's frame
                int frameCount = animationNode.Children.Keys.Where(k => int.TryParse(k, out int blah)).Select(k => int.Parse(k)).DefaultIfEmpty(0).Max() + 1;
                int frameForEntry = (c.Item2.EquipFrame ?? FrameNumber) % frameCount;
                // Resolve for frame, and then ensure the frame is resolved completely. If there is no frame, then the animationNode likely contains the parts
                WZProperty frameNode = animationNode.Resolve(frameForEntry.ToString())?.Resolve() ?? (frameCount == 1 ? animationNode.Resolve() : null);
                if (frameNode == null) return null;
                // Resolve to only children parts that have appropriate locks
                return frameNode.Children.Where(framePart => {
                    // Ensure we're only getting the parts, not the meta attributes that are in the frames
                    WZProperty framePartNode = framePart.Value.Resolve();
                    if (framePartNode == null || framePartNode.Type != PropertyType.Canvas) return false;

                    if(!ElfEars && framePart.Key.Equals("ear", StringComparison.CurrentCultureIgnoreCase)) return false;

                    // If the z-position is equal to the equipCategory, the required locks are the vslot
                    // This seems to resolve the caps only requiring the locks of vslot, not the full `cap` in smap
                    string equipCategory = framePartNode.Path.Split('/')[1];
                    string zPosition = framePartNode.Resolve().ResolveForOrNull<string>("z") ?? framePartNode.ResolveForOrNull<string>("../z") ?? framePartNode.Name;
                    bool sameZAsContainer = !zPosition.Equals(equipCategory, StringComparison.CurrentCultureIgnoreCase);

                    string requiredLockFull = smap.ContainsKey(framePart.Key) && !sameZAsContainer ? smap[framePart.Key] : itemNode.ResolveForOrNull<string>("info/vslot");
                    string[] requiredLocks = Enumerable.Range(0, requiredLockFull.Length / 2).Select(k => requiredLockFull.Substring(k * 2, 2)).ToArray();
                    // Determine if we have locks
                    bool hasLocks = requiredLocks.All(requiredLock => !exclusiveLocks.ContainsKey(requiredLock) || exclusiveLocks[requiredLock] == c.Item2.ItemId);
                    // If we have the lock, we need to ensure we retain the lock to prevent other items from getting the lock

                    // If we don't have the lock and we're assuming we're using the parent's vslot, try using the smap.
                    // This seems to resolve the `hair` z using the more exclusive vslot
                    if (sameZAsContainer && !hasLocks) {
                        requiredLockFull = smap.ContainsKey(framePart.Key) ? smap[framePart.Key] : itemNode.ResolveForOrNull<string>("info/vslot");
                        requiredLocks = Enumerable.Range(0, requiredLockFull.Length / 2).Select(k => requiredLockFull.Substring(k * 2, 2)).ToArray();
                        // Determine if we have locks
                        hasLocks = requiredLocks.All(requiredLock => !exclusiveLocks.ContainsKey(requiredLock) || exclusiveLocks[requiredLock] == c.Item2.ItemId);
                    }

                    if (hasLocks)
                        foreach(string requiredLock in requiredLocks)
                            if (!exclusiveLocks.ContainsKey(requiredLock))
                                exclusiveLocks.Add(requiredLock, c.Item2.ItemId);
                    return hasLocks;
                }).Select(o => o.Value).ToArray();
            })
            .Where(c => c != null)
            .SelectMany(c => c)
            .Concat(Equips.Select(c => { // Concat any effects for items equipped
                WZProperty node = wz.Resolve($"Effect/ItemEff/{c.ItemId}/effect"); // Resolve the selected animation
                if (node == null) return null;
                WZProperty effectNode = node.Resolve(c.AnimationName ?? AnimationName) ?? node.Resolve("default");
                if (effectNode == null) return null;
                int frameCount = effectNode.Children.Keys.Where(k => int.TryParse(k, out int blah)).Select(k => int.Parse(k)).Max();
                int frameForEntry = (c.EquipFrame ?? FrameNumber) % frameCount;
                return effectNode.Resolve(frameForEntry.ToString())?.Resolve();
            }))
            .Where(c => c != null);

            ConcurrentBag<RankedFrame> rankedFrames = new ConcurrentBag<RankedFrame>();

            while(!Parallel.ForEach(frameParts, (c) => {
                string zIndex = c.ResolveForOrNull<string>("../z") ?? c.Resolve().ResolveForOrNull<string>("z");
                int zPosition = 0;
                if (!int.TryParse(zIndex, out zPosition))
                    zPosition = zmap.IndexOf(zIndex);
                RankedFrame ranked = new RankedFrame(Frame.Parse(c), zPosition);

                if (ranked.frame.Position == "face" && !hasFace) return;

                rankedFrames.Add(ranked);
            }).IsCompleted) Thread.Sleep(1);

            return rankedFrames.ToArray();
        }
    }

    public class EquipSelection {
        public int ItemId;
        public string AnimationName;
        public int? EquipFrame;
    }

    public class RankedFrame {
        public readonly Frame frame;
        public readonly int ranking;

        public RankedFrame(Frame frame, int ranking) {
            this.frame = frame;
            this.ranking = ranking;
        }
    }

    public class PositionedFrame {
        public readonly Frame frame;
        public readonly Point position;

        public PositionedFrame(Frame frame, Point position) {
            this.frame = frame;
            this.position = position;
        }
    }

    public enum RenderMode {
        Full,
        Compact,
        Centered
    }
}