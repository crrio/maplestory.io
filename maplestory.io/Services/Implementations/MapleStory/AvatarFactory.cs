using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using maplestory.io.Data.Characters;
using maplestory.io.Data.Images;
using maplestory.io.Data.Items;
using maplestory.io.Models;
using maplestory.io.Services.Interfaces.MapleStory;
using Microsoft.AspNetCore.Http;
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

namespace maplestory.io.Services.Implementations.MapleStory
{
    public class AvatarFactory : IAvatarFactory
    {
        public string[] RequiresFace = new string[] { "face", "capeOverHead", "accessoryEye" };

        public static readonly FontCollection fonts;
        static AvatarFactory()
        {
            fonts = new FontCollection();
            using (FileStream arial = File.OpenRead("assets/Fonts/arial.ttf"))
                fonts.Install(arial);
        }

        public AvatarFactory(IWZFactory wzFactory) => this._wzFactory = wzFactory;
        private IWZFactory _wzFactory;

        void LoadCharacter(Character character, out bool hasChair, out bool hasMount, out string chairSitAction, out int weaponType, out Dictionary<AvatarItemEntry, WZProperty> resolved, out Dictionary<string, int> exclusiveLocks, out List<string> zmap, out Dictionary<string, string> smap, out bool hasFace)
        {
            if (!character.ItemEntries.Any(c => c.ItemId < 10000)) character.ItemEntries = MoreLinq.MoreEnumerable.Append(character.ItemEntries, new AvatarItemEntry() { ItemId = 2000, Alpha = 0 }).ToArray();
            if (!character.ItemEntries.Any(c => c.ItemId < 20000)) character.ItemEntries = MoreLinq.MoreEnumerable.Append(character.ItemEntries, new AvatarItemEntry() { ItemId = 12000, Alpha = 0 }).ToArray();

            WZProperty body = null, head = null;
            hasChair = false;
            hasMount = false;
            chairSitAction = "sit";
            weaponType = 30;
            resolved = ResolveItems(character.ItemEntries, ref body, ref head, ref hasChair, ref hasMount, ref chairSitAction);
            // We need the weapon entry so we know what kind of weapon the character has equipped
            // Certain items require the weapon type to determine what kind of animation will be displayed
            KeyValuePair<AvatarItemEntry, WZProperty>? weaponEntry = resolved.FirstOrDefault(c => c.Value.Parent.NameWithoutExtension.Equals("Weapon"));
            // Default to weapon type `30`
            weaponType = weaponEntry != null && weaponEntry.Value.Value != null ? (int)((weaponEntry.Value.Key.ItemId - 1000000) / 10000d) : 30;
            // WeaponTypes of 70 are cash items, go back to 30.
            if (weaponType == 70)
            {
                weaponEntry = resolved.LastOrDefault(c => c.Value.Parent.NameWithoutExtension.Equals("Weapon"));
                weaponType = weaponEntry != null && weaponEntry.Value.Value != null ? (int)((weaponEntry.Value.Key.ItemId - 1000000) / 10000d) : 30;
                if (weaponType == 70)
                    weaponType = 30;
            }
            exclusiveLocks = ResolveItemLocks(ref resolved, out zmap, out smap);
            hasFace = (body?.Resolve(character.AnimationName) ?? body?.Resolve("default"))?.ResolveFor<bool>($"{character.FrameNumber}/face") ?? true;
        }

        int lcmn(int[] numbers) => numbers.Aggregate(lcm);
        int lcm(int a, int b) => Math.Abs(a * b) / GCD(a, b);
        int GCD(int a, int b) => b == 0 ? a : GCD(b, a % b);

        Tuple<Frame, Point, AvatarItemEntry>[] GetFrameParts(string animationName, int frameNumber, Dictionary<string, Point> anchorPositions, List<KeyValuePair<string, Point>[]> offsets, IEnumerable<RankedFrame<AvatarItemEntry>> animationParts)
        {
            Tuple<Frame, AvatarItemEntry>[] partsFrames = animationParts.Select(c => new Tuple<Frame, AvatarItemEntry>(c.frame, c.underlyingEquip)).ToArray();

            if (anchorPositions == null) anchorPositions = new Dictionary<string, Point>() { { "navel", new Point(0, 0) } };
            else if (!anchorPositions.ContainsKey("navel")) anchorPositions.Add("navel", new Point(0, 0));
            RankedFrame<AvatarItemEntry> bodyFrame = animationParts.FirstOrDefault(c => (c.frame.Position == "body" || c.frame.Position == "backBody") && c.frame.MapOffset.ContainsKey("neck") && c.frame.MapOffset.ContainsKey("navel"));
            Point neckOffsetBody = bodyFrame?.frame.MapOffset["neck"] ?? new Point(0, 0);
            Point navelOffsetBody = bodyFrame?.frame.MapOffset["navel"] ?? new Point(0, 0);

            if (animationName.Equals("alert", StringComparison.CurrentCultureIgnoreCase))
            {
                switch (frameNumber % 3)
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

            Tuple<Frame, Point, AvatarItemEntry>[] positionedFrames = partsFrames.Select(c =>
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

                return new Tuple<Frame, Point, AvatarItemEntry>(
                    c.Item1,
                    new Point(fromAnchorPoint.X - partOrigin.X, fromAnchorPoint.Y - partOrigin.Y),
                    c.Item2
                );
            }).ToArray();

            return positionedFrames;
        }
        IEnumerable<RankedFrame<AvatarItemEntry>> GetAnimationParts(string animationName, int frameNumber, bool elfEars, bool lefEars, bool hasChair, bool hasMount, string chairSitAction, int weaponType, Dictionary<AvatarItemEntry, WZProperty> resolved, Dictionary<string, int> exclusiveLocks, List<string> zmap, Dictionary<string, string> smap, bool HasFace, List<KeyValuePair<string, Point>[]> offsets)
        {
            IEnumerable<Tuple<WZProperty, AvatarItemEntry>> frameParts = resolved.Select(c =>
            {
                WZProperty itemNode = c.Value;
                WZProperty node = itemNode; // Resolve all items and body parts to their correct nodes for the animation
                if (node.Children.Where(n => n.NameWithoutExtension != "info").All(n => int.TryParse(n.NameWithoutExtension, out int blah)))
                    node = node.Resolve($"{weaponType.ToString()}"); // If their selected animation doesn't exist, try ours, and then go to default as a fail-safe

                if (node == null) return null;

                string requiredAnimation = animationName;
                if (hasMount && (animationName != "rope" && animationName != "ladder" && animationName != "sit")) requiredAnimation = "sit";
                if (hasChair) requiredAnimation = chairSitAction;

                WZProperty animationNode = node.Resolve((requiredAnimation != null && (c.Key.ItemId < 1902000 || c.Key.ItemId > 1993000)) ? requiredAnimation : (c.Key.AnimationName ?? animationName)) ??
                    (requiredAnimation != null ? node.Resolve(c.Key.AnimationName ?? animationName) : node.Resolve("default")) ?? node.Resolve("default");
                if (animationNode == null)
                {
                    if (!(c.Key.ItemId >= 1902000 && c.Key.ItemId <= 1993000 && (animationNode = node.Resolve(requiredAnimation ?? "sit")) != null))
                        return null;
                }
                // Resolve to animation's frame
                int frameCount = animationNode.Children.Where(k => int.TryParse(k.NameWithoutExtension, out int blah)).Select(k => int.Parse(k.NameWithoutExtension)).DefaultIfEmpty(0).Max() + 1;
                int frameForEntry = (c.Key.EquipFrame ?? frameNumber) % frameCount;
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

                    if (!elfEars && framePart.NameWithoutExtension.Equals("ear", StringComparison.CurrentCultureIgnoreCase)) return false;
                    if (!lefEars && framePart.NameWithoutExtension.Equals("lefEar", StringComparison.CurrentCultureIgnoreCase)) return false;
                    if (framePart.NameWithoutExtension.Equals("highlefEar", StringComparison.CurrentCultureIgnoreCase)) return false;

                    // If the z-position is equal to the equipCategory, the required locks are the vslot
                    // This seems to resolve the caps only requiring the locks of vslot, not the full `cap` in smap
                    string equipCategory = framePartNode.Path.Split(System.IO.Path.DirectorySeparatorChar)[1];
                    string zPosition = framePartNode.Resolve().ResolveForOrNull<string>("z") ?? framePartNode.ResolveForOrNull<string>("../z") ?? framePartNode.NameWithoutExtension;
                    bool sameZAsContainer = !zPosition.Equals(equipCategory, StringComparison.CurrentCultureIgnoreCase);

                    string requiredLockFull = smap.ContainsKey(framePart.NameWithoutExtension) && !sameZAsContainer ? smap[framePart.NameWithoutExtension] : itemNode.ResolveForOrNull<string>("info/vslot") ?? "";
                    string[] requiredLocks = Enumerable.Range(0, requiredLockFull.Length / 2).Select(k => requiredLockFull.Substring(k * 2, 2)).ToArray();
                    // Determine if we have locks
                    bool hasLocks = requiredLocks.Count() == 0 || requiredLocks.All(requiredLock => !exclusiveLocks.ContainsKey(requiredLock) || exclusiveLocks[requiredLock] == c.Key.ItemId);
                    // If we have the lock, we need to ensure we retain the lock to prevent other items from getting the lock

                    // If we don't have the lock and we're assuming we're using the parent's vslot, try using the smap.
                    // This seems to resolve the `hair` z using the more exclusive vslot
                    if (sameZAsContainer && !hasLocks)
                    {
                        requiredLockFull = smap.ContainsKey(framePart.NameWithoutExtension) ? smap[framePart.NameWithoutExtension] : itemNode.ResolveForOrNull<string>("info/vslot");
                        if ((int)(c.Key.ItemId / 10000) == 104 && requiredLockFull.Equals("MaPn")) requiredLockFull = "Ma";
                        requiredLocks = Enumerable.Range(0, requiredLockFull.Length / 2).Select(k => requiredLockFull.Substring(k * 2, 2)).ToArray();
                        // Determine if we have locks
                        hasLocks = requiredLocks.All(requiredLock => !exclusiveLocks.ContainsKey(requiredLock) || exclusiveLocks[requiredLock] == c.Key.ItemId);
                    }

                    if (hasLocks)
                        foreach (string requiredLock in requiredLocks)
                            if (!exclusiveLocks.ContainsKey(requiredLock))
                                exclusiveLocks.Add(requiredLock, c.Key.ItemId);
                    return hasLocks;
                }).Select(o => new Tuple<WZProperty, AvatarItemEntry>(o, c.Key)).ToArray();
            })
            .Where(c => c != null)
            .SelectMany(c => c)
            .Concat(resolved.Select(c =>
            { // Concat any effects for items equipped
                var wz = _wzFactory.GetWZ(c.Key.Region, c.Key.Version);
                IEnumerable<WZProperty> nodes = new WZProperty[] { wz.Resolve("Effect/ItemEff")?.Resolve($"{c.Key.ItemId}/effect") }; // Resolve the selected animation
                if (nodes.First() == null && (c.Key.ItemId / 10000) == 301) nodes = wz.Resolve("Item/Install/0301")?.Resolve($"{c.Key.ItemId.ToString("D8")}").Children.Where(eff => eff.NameWithoutExtension.StartsWith("effect", StringComparison.CurrentCultureIgnoreCase));

                return nodes?.Where(node => node != null).Select(node =>
                {
                    WZProperty effectNode = node.Resolve(c.Key.AnimationName ?? animationName) ?? node.Resolve("default") ?? (node.Children.Any(b => b.NameWithoutExtension.Equals("0")) ? node : null);
                    if (effectNode == null) return null;
                    int frameCount = effectNode.Children.Where(k => int.TryParse(k.NameWithoutExtension, out int blah)).Select(k => int.Parse(k.NameWithoutExtension)).Max() + 1;
                    int frameForEntry = (c.Key.EquipFrame ?? frameNumber) % frameCount;
                    return new Tuple<WZProperty, AvatarItemEntry>(effectNode.Resolve(frameForEntry.ToString())?.Resolve(), c.Key);
                });
            }).Where(nodes => nodes != null).SelectMany(eff => eff.Where(node => node != null)))
            .Where(c => c != null);

            ConcurrentBag<RankedFrame<AvatarItemEntry>> rankedFrames = new ConcurrentBag<RankedFrame<AvatarItemEntry>>();

            while (!Parallel.ForEach(frameParts ?? new Tuple<WZProperty, AvatarItemEntry>[0], (c) =>
            {
                string zIndex = c.Item1.ResolveForOrNull<string>("../z") ?? c.Item1.Resolve().ResolveForOrNull<string>("z") ?? "0";
                int zPosition = 0;
                if (!int.TryParse(zIndex, out zPosition))
                    zPosition = zmap.IndexOf(zIndex);
                else zPosition = (zPosition - 1) * 500;

                if (!HasFace && zIndex.EndsWith("BelowFace", StringComparison.CurrentCultureIgnoreCase)) zPosition -= 100;

                RankedFrame<AvatarItemEntry> ranked = new RankedFrame<AvatarItemEntry>(Frame.Parse(c.Item1), zPosition, c.Item2);

                if (RequiresFace.Any(b => b.Equals(ranked?.frame?.Position ?? "", StringComparison.CurrentCultureIgnoreCase)) && !HasFace) return;

                rankedFrames.Add(ranked);
            }).IsCompleted) Thread.Sleep(1);

            RankedFrame<AvatarItemEntry>[] rankedFramesArray = new RankedFrame<AvatarItemEntry>[rankedFrames.Count];
            int i = 0;
            while (rankedFrames.TryTake(out RankedFrame<AvatarItemEntry> rankedFrame)) rankedFramesArray[i++] = rankedFrame;
            return rankedFramesArray.OrderBy(c => c.ranking).ToArray();
        }
        Dictionary<string, int> ResolveItemLocks(ref Dictionary<AvatarItemEntry, WZProperty> equipped, out List<string> zmap, out Dictionary<string, string> smap)
        {
            zmap = null;
            smap = null;

            string[] islots = equipped.Values.Select(c => c.ResolveForOrNull<string>("info/islot")?.Substring(0, 2)).ToArray();

            List<string> zmapLocal = null;
            foreach (KeyValuePair<AvatarItemEntry, WZProperty> c in equipped)
            {
                var wz = _wzFactory.GetWZ(c.Key.Region, c.Key.Version);
                List<string> bZmap = (wz.Resolve("Base/zmap") ?? wz.Resolve("zmap")).Children.Select(b => b.NameWithoutExtension).Reverse().ToList();
                if (islots.All(islot => bZmap.IndexOf(islot) != -1))
                {
                    zmap = zmapLocal = bZmap;
                    smap = (wz.Resolve("Base/smap") ?? wz.Resolve("smap")).Children
                        .Where(b => b.ResolveForOrNull<string>() != null)
                        .ToDictionary(b => b.NameWithoutExtension, b => (b.ResolveForOrNull<string>() ?? "").Replace("PnSo", "Pn"));
                    break;
                }
            }

            if (zmapLocal == null) throw new InvalidOperationException("No ZMap found that supports all of the selected items");

            // Build a sorted list of defined exclusive locks from items
            IEnumerable<Tuple<int, string[], string[]>> exclusiveLockItems = equipped
                .OrderBy(c => zmapLocal.IndexOf(c.Value.ResolveForOrNull<string>("info/islot")?.Substring(0, 2)) * ((c.Value.ResolveFor<bool>("info/cash") ?? false) ? 2 : 1))
                .Select(c => {
                    string islot = c.Value.ResolveForOrNull<string>("info/islot") ?? "";
                    string vslot = c.Value.ResolveForOrNull<string>("info/vslot") ?? "";
                    if ((int)(c.Key.ItemId / 10000) == 104)
                    {
                        if (islot.Equals("MaPn")) islot = "Ma"; // No clue why normal shirts would claim to be overalls, but fuck off.
                        if (vslot.Equals("MaPn")) vslot = "Ma"; // No clue why normal shirts would claim to be overalls, but fuck off.
                    }
                    return new Tuple<int, string, string>(
                        c.Key.ItemId,
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
            IEnumerable<KeyValuePair<AvatarItemEntry, WZProperty>> newEquipped = equipped;
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
                    newEquipped = newEquipped.Where(c => c.Key.ItemId != exclusiveLock.Item1);
                }
            }
            equipped = newEquipped.ToDictionary(c => c.Key, c=> c.Value);

            // Build a dictionary between what is locked and what is locking it
            Dictionary<string, int> exclusiveLocks = new Dictionary<string, int>();
            foreach (Tuple<int, string[], string[]> exclusiveLock in exclusiveLockItems)
                if (exclusiveSlots.Any(slot => slot.Value == exclusiveLock.Item1))
                    foreach (string locking in exclusiveLock.Item2)
                        if (exclusiveLocks.ContainsKey(locking))
                            exclusiveLocks[locking] = exclusiveLock.Item1;
                        else
                            exclusiveLocks.Add(locking, exclusiveLock.Item1);

            return exclusiveLocks;
        }
        Dictionary<AvatarItemEntry, WZProperty> ResolveItems(AvatarItemEntry[] itemEntries, ref WZProperty body, ref WZProperty head, ref bool hasChair, ref bool hasMount, ref string chairSitAction)
        {
            Dictionary<AvatarItemEntry, WZProperty> resolved = new Dictionary<AvatarItemEntry, WZProperty>();
            List<AvatarItemEntry> explorativeSearch = new List<AvatarItemEntry>();
            foreach (AvatarItemEntry item in itemEntries)
            {
                MSPackageCollection wz = _wzFactory.GetWZ(item.Region, item.Version);
                hasMount = hasMount || item.ItemId >= 1902000 && item.ItemId <= 1993000;
                bool isChair = item.ItemId / 10000 == 301;
                if (isChair)
                {
                    hasChair = true;
                    chairSitAction = wz.Resolve($"Item/Install/0301.img/{item.ItemId.ToString("D8")}/info/sitAction")?.ResolveForOrNull<string>() ?? "sit";
                }
                bool isBody = item.ItemId < 10000;
                bool isHead = item.ItemId < 20000;

                if (isBody)
                {
                    body = wz.Resolve("Character").Resolve(item.ItemId.ToString("D8"));
                    resolved.Add(item, body);
                }
                else if (isHead)
                {
                    head = wz.Resolve("Character").Resolve(item.ItemId.ToString("D8"));
                    resolved.Add(item, head);
                }
                else
                {
                    int category = item.ItemId / 100;

                    if (category == 30100) resolved.Add(item, wz.Resolve($"Item/Install/0301/{item.ItemId.ToString("D8")}"));
                    else if (category == 50100) resolved.Add(item, wz.Resolve($"Item/Cash/0501/{item.ItemId.ToString("D8")}"));
                    else
                    {
                        if (wz.categoryFolders.TryGetValue(category, out string folder))
                            resolved.Add(item, wz.Resolve($"Character/{folder}/{item.ItemId.ToString("D8")}"));
                        else
                            explorativeSearch.Add(item);
                    }
                }
            }

            if (explorativeSearch.Count > 0)
            {
                foreach (var regionGrouping in explorativeSearch.GroupBy(a => a.Region))
                {
                    foreach (var versionGrouping in regionGrouping.GroupBy(a => a.Version))
                    {
                        string[] searchingFor = versionGrouping.Select(b => b.ItemId.ToString("D8")).ToArray();
                        List<WZProperty> found = new List<WZProperty>();
                        MSPackageCollection wz = _wzFactory.GetWZ(regionGrouping.Key, versionGrouping.Key);
                        IEnumerable<WZProperty> allItems = wz.Resolve("Character").Children.SelectMany(b => b.Children);
                        foreach (WZProperty itemNode in allItems)
                        {
                            if (searchingFor.Contains(itemNode.NameWithoutExtension))
                            {
                                resolved.Add(versionGrouping.First(b => b.ItemId.ToString("D8") == itemNode.NameWithoutExtension), itemNode);
                            }
                        }
                    }
                }
            }

            return resolved.Where(c => c.Value.Children.Count() > 1).ToDictionary(c => c.Key, c => c.Value);
        }

        public byte[] GetSpriteSheet(HttpRequest request, SpriteSheetFormat format, Character character)
        {
            Stopwatch watch = Stopwatch.StartNew();
            IEnumerable<AvatarItemEntry> faceItemEntry = character.ItemEntries.Where(c => c.ItemId >= 20000 && c.ItemId <= 29999);
            Equip face = faceItemEntry.Select(c =>
            {
                ItemFactory itemFactory = new ItemFactory();
                itemFactory.WZ = _wzFactory.GetWZ(c.Region, c.Version);
                return (Equip)itemFactory.Search(c.ItemId);
            }).FirstOrDefault();

            string fileExtension = "png";
            if ((format & SpriteSheetFormat.PDNZip) == SpriteSheetFormat.PDNZip) fileExtension = "zip";

            LoadCharacter(character, out bool hasChair, out bool hasMount, out string chairSitAction, out int weaponType, out Dictionary<AvatarItemEntry, WZProperty> resolved, out Dictionary<string, int> exclusiveLocks, out List<string> zmap, out Dictionary<string, string> smap, out bool hasFace);

            KeyValuePair<AvatarItemEntry, WZProperty> body = resolved.FirstOrDefault(c => c.Key.ItemId < 10000);
            Dictionary<string, int> actions = GetBodyFrameCounts(body.Value);
            // Filter to only the actions that all items have
            actions = actions.Where(c => {
                return resolved.All(b => {
                    // If the item is a face, skip it
                    return (b.Key.ItemId >= 20000 && b.Key.ItemId < 30000) || b.Value.Children.Any(a => {
                        // If the child node doesn't equal the animation name, but it does equal the weapon type:
                        // This node is split into weapon categories, go a layer deeper and try matching any of the node's children to the animation name.
                        return a.Name == c.Key || (a.Name == weaponType.ToString() ? a.Children.Any(d =>
                        {
                            return d.Name == c.Key;
                        }) : false);
                    });
                });
            }).ToDictionary(c => c.Key, c => c.Value);

            List<Func<Tuple<string, byte[]>>> allImages = new List<Func<Tuple<string, byte[]>>>();
            bool isMinimal = (format & SpriteSheetFormat.Minimal) == SpriteSheetFormat.Minimal;

            foreach (string emotion in isMinimal ? new[] { "default" } : face?.FrameBooks?.Keys?.ToArray() ?? new[] { "default" })
            {
                int emotionFrames = isMinimal ? 1 : face?.FrameBooks[emotion]?.frames?.Count() ?? 1;
                foreach (int emotionFrame in Enumerable.Range(0, emotionFrames))
                {
                    foreach (KeyValuePair<string, int> animation in actions)
                    {
                        foreach (int frame in Enumerable.Range(0, animation.Value + 1))
                        {
                            if (watch.ElapsedMilliseconds > 120000) return null;
                            allImages.Add(() => {
                                string path = $"{animation.Key}_{frame}.{fileExtension}";
                                if (!isMinimal) path = $"{emotion}/{emotionFrame}/" + path;
                                // We can modify the equips array, but if we change the actual contents other than the face there could be problems.
                                Dictionary<AvatarItemEntry, WZProperty> realResolved = resolved.ToDictionary(c => {
                                    var item = new AvatarItemEntry(c.Key);
                                    item.AnimationName = c.Key.ItemId == face?.id ? emotion : c.Key.AnimationName;
                                    return item;
                                }, c => c.Value);

                                byte[] data = null;
                                if ((format & SpriteSheetFormat.PDNZip) == SpriteSheetFormat.PDNZip)
                                    data = RenderFrameZip(request, character.Mode, animation.Key, character.Name, frame, character.ElfEars, character.LefEars, character.FlipX, character.Zoom, character.Padding, null, hasChair, hasMount, chairSitAction, weaponType, realResolved, exclusiveLocks, zmap, smap, hasFace);
                                else
                                    data = RenderFrame(character.Mode, animation.Key, character.Name, frame, character.ElfEars, character.LefEars, character.FlipX, character.Zoom, character.Padding, null, hasChair, hasMount, chairSitAction, weaponType, realResolved, exclusiveLocks, zmap, smap, hasFace).ImageToByte(request, false);

                                var res = new Tuple<string, byte[]>(
                                    path,
                                    data
                                );
                                return res;
                            });
                        }
                    }
                }
            }

            if (isMinimal && face != null)
            {
                foreach (string emotion in face?.FrameBooks?.Keys?.ToArray() ?? new[] { "default" })
                {
                    int frameNumber = 0;
                    foreach (EquipFrame frame in face?.FrameBooks[emotion]?.frames)
                    {
                        foreach (KeyValuePair<string, Frame> framePart in frame.Effects)
                        {
                            int iFrameNumber = frameNumber;
                            allImages.Add(() => {
                                byte[] bytes = framePart.Value.Image.ImageToByte(request, false);
                                string path = $"faces/{emotion}_{iFrameNumber}_{framePart.Key}.png";
                                return new Tuple<string, byte[]>(path, bytes);
                            });
                        }
                        ++frameNumber;
                    }
                }
            }

            using (MemoryStream mem = new MemoryStream())
            {
                using (ZipArchive archive = new ZipArchive(mem, ZipArchiveMode.Create, true))
                {
                    ConcurrentBag<Tuple<string, byte[]>> bag = new ConcurrentBag<Tuple<string, byte[]>>();
                    Parallel.ForEach(allImages, (a) => {
                        var b = a();
                        if (b == null) return;
                        bag.Add(new Tuple<string, byte[]>(b.Item1, b.Item2));
                    });

                    foreach (Tuple<string, byte[]> frameData in bag)
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

        private byte[] RenderFrameZip(HttpRequest request, RenderMode mode, string animationName, string name, int frameNumber, bool elfEars, bool lefEars, bool flipX, float zoom, int padding, object p, bool hasChair, bool hasMount, string chairSitAction, int weaponType, Dictionary<AvatarItemEntry, WZProperty> resolved, Dictionary<string, int> exclusiveLocks, List<string> zmap, Dictionary<string, string> smap, bool hasFace)
        {
            WZProperty bodyNode = resolved.FirstOrDefault(c => c.Key.ItemId < 10000).Value;
            WZProperty bodyAnimationNode = bodyNode.Resolve($"{animationName}");
            int maxFrame = bodyAnimationNode.Children.Select(c => int.TryParse(c.Name, out int maxFrameNumber) ? maxFrameNumber : -1).Max();
            WZProperty bodyFrameNode = bodyAnimationNode.Resolve((frameNumber % (maxFrame + 1)).ToString());
            string bodyAnimation = bodyAnimationNode.ResolveForOrNull<string>("action");
            int? bodyFrameNumber = bodyAnimationNode.ResolveFor<int>("frame");
            bool? bodyFlip = bodyAnimationNode.ResolveFor<bool>("flip");
            // Override the original name of the animation with the correct `real` animation
            if (bodyAnimation != null) animationName = bodyAnimation;
            if (bodyFrameNumber.HasValue) frameNumber = bodyFrameNumber.Value;
            if (bodyFlip.HasValue) flipX = !flipX;

            List<KeyValuePair<string, Point>[]> offsets = new List<KeyValuePair<string, Point>[]>();

            // Resolve to action nodes and then to frame nodes
            Dictionary<string, Point> anchorPositions = new Dictionary<string, Point>();
            IEnumerable<RankedFrame<AvatarItemEntry>> animationParts = GetAnimationParts(animationName, frameNumber, elfEars, lefEars, hasChair, hasMount, chairSitAction, weaponType, resolved, exclusiveLocks, zmap, smap, hasFace, offsets);
            Tuple<Frame, Point, AvatarItemEntry>[] positionedFrames = GetFrameParts(animationName, frameNumber, anchorPositions, offsets, animationParts).Where(c => c != null).ToArray();

            float minX = positionedFrames.Select(c => c.Item2.X).Min();
            float maxX = positionedFrames.Select(c => c.Item2.X + c.Item1.Image.Width).Max();
            float minY = positionedFrames.Select(c => c.Item2.Y).Min();
            float maxY = positionedFrames.Select(c => c.Item2.Y + c.Item1.Image.Height).Max();
            Size center = new Size((int)((maxX - minX) / 2), (int)((maxY - minY) / 2));
            Size offset = new Size((int)minX, (int)minY);

            Tuple<Frame, Point, AvatarItemEntry> bodyFrame = positionedFrames.Where(c => ((c.Item1.Position?.Equals("body") ?? false) || (c.Item1.Position?.Equals("backBody") ?? false)) && c.Item1.MapOffset.ContainsKey("neck") && c.Item1.MapOffset.ContainsKey("navel")).First();

            float NameWidthAdjustmentX = 0;
            Tuple<Frame, Image<Rgba32>>[] parts = positionedFrames.Select((frame, index) =>
            {
                Image<Rgba32> destination = new Image<Rgba32>((int)((maxX - minX) + (padding * 2)), (int)((maxY - minY) + (padding * 2)));
                destination.Mutate(x =>
                {
                    Image<Rgba32> framePart = frame.Item1.Image.Clone();
                    framePart = DrawFramePart(padding, x, frame, minX, minY, framePart);
                    framePart.Dispose();
                });

                Point feetCenter = new Point();
                return new Tuple<Frame, Image<Rgba32>>(frame.Item1, Transform(mode, flipX, zoom, name, padding, destination, resolved, bodyFrame, minX, minY, maxX, maxY, ref NameWidthAdjustmentX, ref feetCenter, index == 0));
            }).ToArray();

            using (MemoryStream mem = new MemoryStream())
            {
                using (ZipArchive archive = new ZipArchive(mem, ZipArchiveMode.Create, true))
                {
                    ZipArchiveEntry PaintDotNet = archive.CreateEntry("PaintDotNet.txt", CompressionLevel.NoCompression);
                    using (Stream sig = PaintDotNet.Open())
                        sig.Write(Encoding.UTF8.GetBytes("PDN3"), 0, 4);

                    ConcurrentBag<Tuple<string, byte[]>> bag = new ConcurrentBag<Tuple<string, byte[]>>(parts.Select((c, i) => new Tuple<string, byte[]>($"L{i + 1},R1,C1,{c.Item1.Position},visible,normal,255.png", c.Item2.ImageToByte(request, false))));

                    while (bag.TryTake(out Tuple<string, byte[]> frameData))
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

        public Tuple<Image<Rgba32>, Dictionary<string, Point>, Dictionary<string, int>, int> Details(Character character)
        {
            Dictionary<string, Point> anchorPositions = new Dictionary<string, Point>();
            LoadCharacter(character, out bool hasChair, out bool hasMount, out string chairSitAction, out int weaponType, out Dictionary<AvatarItemEntry, WZProperty> resolved, out Dictionary<string, int> exclusiveLocks, out List<string> zmap, out Dictionary<string, string> smap, out bool hasFace);

            KeyValuePair<AvatarItemEntry, WZProperty> body = resolved.FirstOrDefault(c => c.Key.ItemId < 10000);
            WZProperty bodyAnimationNode = body.Value.Resolve(character.AnimationName) ?? body.Value.Resolve("default");
            int maxFrame = bodyAnimationNode.Children.Select(c => int.TryParse(c.Name, out int frameNumber) ? frameNumber : -1).Max();
            int animationDelay = bodyAnimationNode.ResolveFor<int>($"{character.FrameNumber % (maxFrame + 1)}/delay") ?? 0;

            return new Tuple<Image<Rgba32>, Dictionary<string, Point>, Dictionary<string, int>, int>(
                RenderFrame(character.Mode, character.AnimationName, character.Name, character.FrameNumber, character.ElfEars, character.LefEars, character.FlipX, character.Zoom, character.Padding, anchorPositions, hasChair, hasMount, chairSitAction, weaponType, resolved, exclusiveLocks, zmap, smap, hasFace),
                anchorPositions,
                GetBodyFrameCounts(body.Value),
                animationDelay
            );
        }

        public Dictionary<string, int> GetBodyFrameCounts(WZProperty bodyNode) => bodyNode.Resolve("../Hair/00030000").Children.Where(c => c.Name != "default" && c.Name != "backDefault" && c.Name != "info" && c.Name != "heal").ToDictionary(c => c.Name, c => c.Children.Count());
        public Dictionary<string, int> GetPossibleActions(AvatarItemEntry[] items)
        {
            WZProperty body = null, head = null;
            bool hasChair = false, hasMount = false;
            string chairSitAction = null;
            Dictionary<AvatarItemEntry, WZProperty> resolved = ResolveItems(items, ref body, ref head, ref hasChair, ref hasMount, ref chairSitAction);

            return resolved.Aggregate(null, new Func<string[], KeyValuePair<AvatarItemEntry, WZProperty>, string[]>((a, b) =>
            {
                if (b.Value == null) return a;
                else if (a == null) return b.Value.Children.Select(c => c.Name).ToArray();
                else if (b.Key.ItemId >= 20000 && b.Key.ItemId < 30000) return a;
                else return b.Value.Children.Count() > 1 ? b.Value.Children.Select(c => c.Name).Where(c => a.Contains(c)).ToArray() : a;
            })).Where(c => c != "info" && c != "heal").ToDictionary(c => c, animationName =>
            {
                WZProperty bodyAnimationNode = body.Resolve(animationName);
                return bodyAnimationNode.Children.Select(c => int.TryParse(c.Name, out int frameNumber) ? frameNumber : -1).Max();
            });
        }

        public Image<Rgba32> Animate(Character character, Rgba32? background = null)
        {
            LoadCharacter(character, out bool hasChair, out bool hasMount, out string chairSitAction, out int weaponType, out Dictionary<AvatarItemEntry, WZProperty> resolved, out Dictionary<string, int> exclusiveLocks, out List<string> zmap, out Dictionary<string, string> smap, out bool hasFace);

            KeyValuePair<AvatarItemEntry, WZProperty> body = resolved.FirstOrDefault(c => c.Key.ItemId < 10000);
            WZProperty bodyAnimationNode = body.Value.Resolve(character.AnimationName) ?? body.Value.Resolve("default");
            int maxFrame = bodyAnimationNode.Children.Select(c => int.TryParse(c.Name, out int frameNumber) ? frameNumber : -1).Max();

            Tuple<Image<Rgba32>, Dictionary<string, Point>, int>[] frames = Enumerable.Range(0, maxFrame + 1).Select(frameNumber => {
                Dictionary<string, Point> anchorPositions = new Dictionary<string, Point>();
                return new Tuple<Image<Rgba32>, Dictionary<string, Point>, int>(
                    RenderFrame(character.Mode, character.AnimationName, character.Name, frameNumber, character.ElfEars, character.LefEars, character.FlipX, character.Zoom, character.Padding, anchorPositions, hasChair, hasMount, chairSitAction, weaponType, resolved, exclusiveLocks, zmap, smap, hasFace),
                    anchorPositions,
                    bodyAnimationNode.ResolveFor<int>($"{frameNumber}/delay") ?? 0
                );
            }).ToArray();

            // Idle positions 
            if (character.AnimationName.Equals("alert", StringComparison.CurrentCultureIgnoreCase) || character.AnimationName.StartsWith("stand", StringComparison.CurrentCultureIgnoreCase))
                frames = frames.Concat(MoreEnumerable.SkipLast(frames.Reverse().Skip(1), 1)).ToArray();

            int maxWidth = frames.Max(x => x.Item1.Width);
            int maxHeight = frames.Max(x => x.Item1.Height);
            Point maxFeetCenter = new Point(
                frames.Select(c => c.Item2["feetCenter"].X).Max(),
                frames.Select(c => c.Item2["feetCenter"].Y).Max()
            );
            Point maxDifference = new Point(
                maxFeetCenter.X - frames.Select(c => c.Item2["feetCenter"].X).Min(),
                maxFeetCenter.Y - frames.Select(c => c.Item2["feetCenter"].Y).Min()
            );

            List<Image<Rgba32>> pendingDispose = new List<Image<Rgba32>>();
            var gif = new Image<Rgba32>(maxWidth + maxDifference.X, maxHeight + maxDifference.Y);

            for (int i = 0; i < frames.Length; ++i)
            {
                Image<Rgba32> frameImage = frames[i].Item1;
                Point feetCenter = frames[i].Item2["feetCenter"];
                Point offset = new Point(maxFeetCenter.X - feetCenter.X, maxFeetCenter.Y - feetCenter.Y);

                if (offset.X != 0 || offset.Y != 0)
                {
                    Image<Rgba32> offsetFrameImage = new Image<Rgba32>(gif.Width, gif.Height);
                    offsetFrameImage.Mutate(x => x.DrawImage(frameImage, 1, offset));
                    frameImage = offsetFrameImage;
                }

                if (frameImage.Width != gif.Width || frameImage.Height != gif.Height) frameImage.Mutate(x => x.Crop(gif.Width, gif.Height));

                if (background?.A != 0)
                {
                    Image<Rgba32> frameWithBackground = new Image<Rgba32>(frameImage.Width, frameImage.Height);
                    frameWithBackground.Mutate(x =>
                    {
                        x.Fill(background.Value);
                        x.DrawImage(frameImage, 1, Point.Empty);
                    });

                    if (frameImage != frames[i].Item1) frameImage.Dispose();
                    frameImage = frameWithBackground;
                }

                ImageFrame<Rgba32> resultFrame = gif.Frames.AddFrame(frameImage.Frames.RootFrame);
                resultFrame.MetaData.FrameDelay = frames[i].Item3 / 10;
                resultFrame.MetaData.DisposalMethod = SixLabors.ImageSharp.Formats.Gif.DisposalMethod.RestoreToBackground;

                pendingDispose.Add(frameImage);
                if (frameImage != frames[i].Item1) pendingDispose.Add(frames[i].Item1);
            }
            gif.Frames.RemoveFrame(0);

            pendingDispose.ForEach(c => c.Dispose());

            return gif;
        }

        public Image<Rgba32> Render(Character character)
        {
            Dictionary<string, Point> anchorPositions = new Dictionary<string, Point>();
            LoadCharacter(character, out bool hasChair, out bool hasMount, out string chairSitAction, out int weaponType, out Dictionary<AvatarItemEntry, WZProperty> resolved, out Dictionary<string, int> exclusiveLocks, out List<string> zmap, out Dictionary<string, string> smap, out bool hasFace);

            return RenderFrame(character.Mode, character.AnimationName, character.Name, character.FrameNumber, character.ElfEars, character.LefEars, character.FlipX, character.Zoom, character.Padding, anchorPositions, hasChair, hasMount, chairSitAction, weaponType, resolved, exclusiveLocks, zmap, smap, hasFace);
        }

        private Image<Rgba32> RenderFrame(RenderMode mode, string animationName, string name, int frameNumber, bool elfEars, bool lefEars, bool flipX, float zoom, int padding, Dictionary<string, Point> anchorPositions, bool hasChair, bool hasMount, string chairSitAction, int weaponType, Dictionary<AvatarItemEntry, WZProperty> resolved, Dictionary<string, int> exclusiveLocks, List<string> zmap, Dictionary<string, string> smap, bool hasFace)
        {
            if (animationName == null)
                animationName = GetPossibleActions(resolved.Keys.ToArray()).Keys.FirstOrDefault(c => c.StartsWith("stand"));
            WZProperty bodyNode = resolved.FirstOrDefault(c => c.Key.ItemId < 10000).Value;
            WZProperty bodyAnimationNode = bodyNode.Resolve($"{animationName}");
            int maxFrame = bodyAnimationNode.Children.Select(c => int.TryParse(c.Name, out int maxFrameNumber) ? maxFrameNumber : -1).Max();
            WZProperty bodyFrameNode = bodyAnimationNode.Resolve((frameNumber % (maxFrame + 1)).ToString());
            string bodyAnimation = bodyAnimationNode.ResolveForOrNull<string>("action");
            int? bodyFrameNumber = bodyAnimationNode.ResolveFor<int>("frame");
            bool? bodyFlip = bodyAnimationNode.ResolveFor<bool>("flip");
            // Override the original name of the animation with the correct `real` animation
            if (bodyAnimation != null) animationName = bodyAnimation;
            if (bodyFrameNumber.HasValue) frameNumber = bodyFrameNumber.Value;
            if (bodyFlip.HasValue) flipX = !flipX;

            List<KeyValuePair<string, Point>[]> offsets = new List<KeyValuePair<string, Point>[]>();

            // Resolve to action nodes and then to frame nodes
            IEnumerable<RankedFrame<AvatarItemEntry>> animationParts = GetAnimationParts(animationName, frameNumber, elfEars, lefEars, hasChair, hasMount, chairSitAction, weaponType, resolved, exclusiveLocks, zmap, smap, hasFace, offsets);
            Tuple<Frame, Point, AvatarItemEntry>[] positionedFrames = GetFrameParts(animationName, frameNumber, anchorPositions, offsets, animationParts).Where(c => c != null).ToArray();

            float minX = positionedFrames.Select(c => c.Item2.X).Min();
            float maxX = positionedFrames.Select(c => c.Item2.X + c.Item1.Image.Width).Max();
            float minY = positionedFrames.Select(c => c.Item2.Y).Min();
            float maxY = positionedFrames.Select(c => c.Item2.Y + c.Item1.Image.Height).Max();
            Size center = new Size((int)((maxX - minX) / 2), (int)((maxY - minY) / 2));
            Size offset = new Size((int)minX, (int)minY);

            Image<Rgba32> destination = new Image<Rgba32>((int)((maxX - minX) + (padding * 2)), (int)((maxY - minY) + (padding * 2)));
            destination.Mutate(x => positionedFrames.ForEach(frame =>
            {
                Image<Rgba32> framePart = frame.Item1.Image.Clone();
                framePart = DrawFramePart(padding, x, frame, minX, minY, framePart);
                framePart.Dispose();
            }));

            Tuple<Frame, Point, AvatarItemEntry> bodyFrame = positionedFrames.Where(c => ((c.Item1.Position?.Equals("body") ?? false) || (c.Item1.Position?.Equals("backBody") ?? false)) && c.Item1.MapOffset.ContainsKey("neck") && c.Item1.MapOffset.ContainsKey("navel")).First();

            float NameWidthAdjustmentX = 0;
            Point feetCenter = new Point();
            Image<Rgba32> res = Transform(mode, flipX, zoom, name, padding, destination, resolved, bodyFrame, minX, minY, maxX, maxY, ref NameWidthAdjustmentX, ref feetCenter);
            anchorPositions?.Add("feetCenter", feetCenter);

            return res;
        }

        private static Image<Rgba32> DrawFramePart(int padding, IImageProcessingContext<Rgba32> x, Tuple<Frame, Point, AvatarItemEntry> frame, float minX, float minY, Image<Rgba32> framePart)
        {
            if (frame.Item3.Hue.HasValue)
                using (Image<Rgba32> oldFramePart = framePart)
                {
                    framePart = oldFramePart.Clone();
                    framePart.Mutate(c => c.Hue(frame.Item3.Hue.Value));
                }
            if (frame.Item3.Contrast.HasValue)
                using (Image<Rgba32> oldFramePart = framePart)
                {
                    framePart = oldFramePart.Clone();
                    framePart.Mutate(c => c.Contrast(frame.Item3.Contrast.Value));
                }
            if (frame.Item3.Saturation.HasValue)
                using (Image<Rgba32> oldFramePart = framePart)
                {
                    framePart = oldFramePart.Clone();
                    framePart.Mutate(c => c.Saturate(frame.Item3.Saturation.Value));
                }
            if (frame.Item3.Brightness.HasValue)
                using (Image<Rgba32> oldFramePart = framePart)
                {
                    framePart = oldFramePart.Clone();
                    framePart.Mutate(c => c.Contrast(frame.Item3.Brightness.Value));
                }
            if (frame.Item3.Alpha.HasValue)
                using (Image<Rgba32> oldFramePart = framePart)
                {
                    framePart = oldFramePart.Clone();
                    framePart.Mutate(c => c.Opacity(frame.Item3.Alpha.Value));
                }
            x.DrawImage(
                framePart,
                1,
                new Point(
                    (int)(frame.Item2.X - minX) + padding,
                    (int)(frame.Item2.Y - minY) + padding
                )
            );
            return framePart;
        }

        Image<Rgba32> Transform(RenderMode mode, bool flipX, float zoom, string name, int padding, Image<Rgba32> destination, Dictionary<AvatarItemEntry, WZProperty> resolved, Tuple<Frame, Point, AvatarItemEntry> body, float minX, float minY, float maxX, float maxY, ref float NameWidthAdjustmentX, ref Point feetCenter, bool includeName = true)
        {
            if (mode == RenderMode.Compact)
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
            else if (mode == RenderMode.Centered)
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
            else if (mode == RenderMode.NavelCenter)
            {
                Point imageCenter = new Point(destination.Width / 2, destination.Height / 2);
                Point distanceFromCen = Point.Add(imageCenter, new Size((int)minX, (int)minY));
                Point distanceFromCenter = new Point(distanceFromCen.X * 2, distanceFromCen.Y * 2);
                Image<Rgba32> centered = new Image<Rgba32>(destination.Width + (int)Math.Abs(distanceFromCenter.X), destination.Height + (int)Math.Abs(distanceFromCenter.Y));
                centered.Mutate(c => c.DrawImage(destination, 1, new Point((int)Math.Max(distanceFromCenter.X, 0), (int)Math.Max(distanceFromCenter.Y, 0))));
                destination.Dispose();

                return centered;
            }
            else if (mode == RenderMode.FeetCenter)
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

            if (flipX || zoom != 1)
            {
                if (flipX) destination.Mutate(x => { if (flipX) x.Flip(FlipMode.Horizontal); });
                if (zoom != 1 && zoom != 0)
                {
                    if ((destination.Height * zoom) < 50000 && (destination.Width * zoom) < 50000)
                    {
                        destination.Mutate(c => c.Resize(new ResizeOptions()
                        {
                            Mode = ResizeMode.Stretch,
                            Sampler = new NearestNeighborResampler(),
                            Size = new Size((int)(destination.Width * zoom), (int)(destination.Height * zoom))
                        }));
                    }
                }
            }

            if (includeName && !string.IsNullOrEmpty(name))
            {
                if (name.Length > 64) name = name.Substring(0, 64);

                IEnumerable<KeyValuePair<AvatarItemEntry, WZProperty>> rings = resolved.Where(l => (l.Key.ItemId / 1000) == 1112);
                Tuple<int?, WZProperty> labelRing = rings.Select(l =>
                {
                    return new Tuple<int?, WZProperty>(l.Value.ResolveFor<int>("info/nameTag"), l.Value);
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

                feetCenter = Point.Add(calcFeetCenter(flipX, zoom, ref NameWidthAdjustmentX, body, minX, minY, destination), new Size((int)(padding * zoom), (int)(padding * zoom)));
                Font MaplestoryFont = fonts.Families
                    .First(f => f.Name.Equals("Arial Unicode MS", StringComparison.CurrentCultureIgnoreCase)).CreateFont(12, FontStyle.Regular);
                SizeF realNameSize = TextMeasurer.Measure(name, new RendererOptions(MaplestoryFont));
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
                        x.DrawText(new TextGraphicsOptions() { VerticalAlignment = VerticalAlignment.Center }, name, MaplestoryFont, nameColor, PointF.Add(textPosition, new PointF(0, realNameSize.Height / 2f + 1)));
                    }
                    else
                    {
                        x.DrawImage(w, 1, new Point((int)textPosition.X - wOrigin.X, (int)textPosition.Y - wOrigin.Y));
                        using (var cv = c.Clone(v => v.Resize(new Size((boxPosition.Width) - (w.Width + e.Width), c.Height))))
                            x.DrawImage(cv, 1, new Point((int)(textPosition.X) - cOrigin.X, (int)textPosition.Y - cOrigin.Y));
                        x.DrawImage(e, 1, new Point((int)(textPosition.X + boxPosition.Width - (w.Width + e.Width)), (int)textPosition.Y - eOrigin.Y));
                        x.DrawText(new TextGraphicsOptions() { VerticalAlignment = VerticalAlignment.Center }, name, MaplestoryFont, nameColor, PointF.Add(textPosition, new PointF(0, realNameSize.Height / 2f - 1)));
                    }
                    x.DrawImage(destination, 1, new Point((int)Math.Round(-nMinX), 0));
                });
                destination.Dispose();

                return withName;
            }
            else feetCenter = Point.Add(calcFeetCenter(flipX, zoom, ref NameWidthAdjustmentX, body, minX, minY, destination), new Size((int)(padding * zoom), (int)(padding * zoom)));

            return destination;
        }

        Point calcFeetCenter(bool flipX, float zoom, ref float NameWidthAdjustmentX, Tuple<Frame, Point, AvatarItemEntry> body, float minX, float minY, Image<Rgba32> destination)
        {
            Point bodyOrigin = body.Item1.OriginOrZero;
            Point feetCenter = new Point(
                (int)(((body.Item2.X - minX) + bodyOrigin.X) - NameWidthAdjustmentX) - 4,
                (int)((body.Item2.Y - minY) + bodyOrigin.Y)
            );
            feetCenter = new Point((int)(feetCenter.X * zoom), (int)(feetCenter.Y * zoom));
            if (flipX) feetCenter.X = destination.Width - feetCenter.X;
            return feetCenter;
        }

        IPathCollection BuildCorners(int x, int y, int width, int height, float cornerRadius)
        {
            // first create a square
            var rect = new RectangularPolygon(x - 0.5f, y - 0.5f, cornerRadius, cornerRadius);

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
    }
}
