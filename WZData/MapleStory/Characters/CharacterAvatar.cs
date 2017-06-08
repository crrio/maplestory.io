using MoreLinq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using WZData.MapleStory.Images;
using WZData.MapleStory.Items;

namespace WZData.MapleStory.Characters
{
    public class CharacterAvatar
    {
        public bool ShowEars;
        public string AnimationName;
        public int Frame;
        public int Padding;
        public IEnumerable<Tuple<MapleItem, string>> Items;
        public readonly CharacterSkin BaseSkin;
        public Dictionary<string, Point> anchorPositions = new Dictionary<string, Point>() { { "navel", new Point(0, 0) } };

        public bool HasFace { get => EntireBodyFrame.HasFace ?? false; }

        public BodyAnimation SkinAnimation { get => BaseSkin.Animations[AnimationName]; }
        public Body EntireBodyFrame { get => SkinAnimation.Frames[Frame % SkinAnimation.Frames.Length]; }
        public Dictionary<string, BodyPart> BodyParts { get => EntireBodyFrame.Parts; }

        public IEnumerable<Tuple<Equip, string>> EquipFramesSelected
        {
            get => Items.Where(c => c.Item1 is Equip)
                .Select(c => new Tuple<Equip, string>((Equip)c.Item1, c.Item2))
                .Where(c => c.Item1.FrameBooks.Count > 0);
        }
        public IEnumerable<Equip> Equips { get => EquipFramesSelected.Select(c => c.Item1); }
        public int WeaponCategory {
            get => (int)Math.Floor(
                ((EquipFramesSelected
                    .Where(c => c.Item1.EquipGroup.Equals("weapon", StringComparison.CurrentCultureIgnoreCase) && !c.Item1.MetaInfo.Cash.cash)
                    .FirstOrDefault()?.Item1.id ?? 0
                ) - 1000000) / 10000d
            );
        }

        public IEnumerable<Tuple<Equip, string, IFrame>> EquipFrames
        {
            get => EquipFramesSelected
                .GroupBy(c => c.Item1.EquipGroup)
                .Select(c => c.FirstOrDefault(b => b.Item1.MetaInfo.Cash?.cash ?? false) ?? c.First())
                // Some equips aren't always shown, like weapons when sitting
                .Where(c => c.Item1.GetFrameBooks(WeaponCategory).ContainsKey(c.Item2 ?? AnimationName) || c.Item1.GetFrameBooks(WeaponCategory).ContainsKey("default"))
                .Select(c => new Tuple<Equip, EquipFrameBook>(c.Item1, c.Item1.GetFrameBooks(WeaponCategory).ContainsKey(c.Item2 ?? AnimationName) ? c.Item1.GetFrameBooks(WeaponCategory)[c.Item2 ?? AnimationName] : c.Item1.GetFrameBooks(WeaponCategory)["default"]))
                .Select(c => new Tuple<Equip, EquipFrame>(c.Item1, c.Item2.frames.Count() <= Frame ? c.Item2.frames.ElementAt(Frame % c.Item2.frames.Count()) : c.Item2.frames.ElementAt(Frame)))
                .SelectMany(c => c.Item2.Effects.Select(b => new Tuple<Equip, string, IFrame>(c.Item1, b.Key, b.Value)));
        }

        public IEnumerable<IFrame> EffectFrames
        {
            get => EquipFramesSelected
                .Where(c => c.Item1.ItemEffects?.entries?.Count > 0)
                .Where(c => c.Item1.ItemEffects.entries.ContainsKey(c.Item2 ?? AnimationName) || c.Item1.ItemEffects.entries.ContainsKey("default"))
                .Select(c => new Tuple<Equip, FrameBook>(c.Item1, c.Item1.ItemEffects.entries.ContainsKey(c.Item2 ?? AnimationName) ? c.Item1.ItemEffects.entries[c.Item2 ?? AnimationName].FirstOrDefault() : c.Item1.ItemEffects.entries["default"].FirstOrDefault()))
                .Where(c => c.Item2 != null)
                .Select(c => new Tuple<Equip, IFrame>(c.Item1, c.Item2.frames.Count() <= Frame ? c.Item2.frames.ElementAt(Frame % c.Item2.frames.Count()) : c.Item2.frames.ElementAt(Frame)))
                .Where(c => c != null && int.TryParse(c.Item2.Position, out int blah))
                .GroupBy(c => c.Item2.Position)
                .Select(c => c.FirstOrDefault())
                .GroupBy(c => c.Item1.EquipGroup)
                .Select(c => c.FirstOrDefault(b => b.Item1.MetaInfo.Cash?.cash ?? false) ?? c.First())
                .Select(c => c.Item2);
        }

        public CharacterAvatar(CharacterSkin baseSkin)
        {
            this.BaseSkin = baseSkin;
        }

        public List<Tuple<string, Point, IFrame>> GetElementPieces(ZMap zmapping, SMap smapping, List<IFrame> frames = null)
        {
            if (frames == null)
                frames = GetBodyPieces(zmapping, smapping).ToList();

            List<Tuple<string, Point, IFrame>> elements = new List<Tuple<string, Point, IFrame>>();

            while (frames.Count > 0)
            {
                IFrame part = frames.Where(c => c.MapOffset?.Any(b => anchorPositions.ContainsKey(b.Key)) ?? false).FirstOrDefault() ?? frames.First();

                Point partOrigin = part.Origin ?? Point.Empty;
                Point withOffset = Point.Empty;
                if (part.MapOffset != null)
                {
                    KeyValuePair<string, Point> anchorPointEntry = part.MapOffset.Where(c => anchorPositions.ContainsKey(c.Key)).First();
                    Point anchorPoint = anchorPositions[anchorPointEntry.Key];
                    Point vectorFromPoint = anchorPointEntry.Value;
                    Point fromAnchorPoint = new Point(anchorPoint.X - vectorFromPoint.X, anchorPoint.Y - vectorFromPoint.Y);

                    foreach (KeyValuePair<string, Point> childAnchorPoint in part.MapOffset.Where(c => c.Key != anchorPointEntry.Key))
                    {
                        Point resultAnchorPoint = new Point(fromAnchorPoint.X + childAnchorPoint.Value.X, fromAnchorPoint.Y + childAnchorPoint.Value.Y);
                        if (!anchorPositions.ContainsKey(childAnchorPoint.Key))
                            anchorPositions.Add(childAnchorPoint.Key, resultAnchorPoint);
                        else if (anchorPositions[childAnchorPoint.Key].X != resultAnchorPoint.X || anchorPositions[childAnchorPoint.Key].Y != resultAnchorPoint.Y)
                            throw new InvalidOperationException("Duplicate anchor point, but position doesn't match up, possible state corruption?");
                    }

                    withOffset = new Point(fromAnchorPoint.X - partOrigin.X, fromAnchorPoint.Y - partOrigin.Y);
                }
                else
                {
                    Point neckPoint = (Point?)anchorPositions.FirstOrDefault(c => c.Key == "brow").Value ?? new Point(0, 0);
                    withOffset = new Point(neckPoint.X - partOrigin.X, neckPoint.Y - partOrigin.Y);
                }

                elements.Add(new Tuple<string, Point, IFrame>(part.Position, withOffset, part));
                frames.Remove(part);
            }

            return elements;
        }

        IEnumerable<IFrame> GetBodyPieces(ZMap zmapping, SMap smapping)
        {
            Dictionary<string, Equip> boundLayers = new Dictionary<string, Equip>();
            List<Tuple<Equip, IFrame, string[]>> requiredLayers = new List<Tuple<Equip, IFrame, string[]>>();

            foreach (Tuple<string, IEnumerable<Tuple<Equip, string, IFrame>>> eqp in zmapping.Ordering
                .Where(c => (EntireBodyFrame.HasFace ?? true) || c != "face")
                .Select(c => new Tuple<string, IEnumerable<Tuple<Equip, string, IFrame>>>(c, EquipFrames.Where(b => b.Item1.MetaInfo.Equip.islots.Contains(c))))
                .Where(c => c.Item2 != null))
            {
                string currentZ = eqp.Item1;
                foreach (Tuple<Equip, string, IFrame> underlyingFrame in eqp.Item2)
                {
                    Equip currentEquip = underlyingFrame.Item1;
                    string framePosition = underlyingFrame.Item2;
                    IFrame currentFrame = underlyingFrame.Item3;

                    foreach (string explicitSlot in currentEquip.MetaInfo.Equip.vslots)
                    {
                        if (boundLayers.ContainsKey(explicitSlot))
                            boundLayers[explicitSlot] = currentEquip;
                        else
                            boundLayers.Add(explicitSlot, currentEquip);

                        if (framePosition != "default" && (!(EntireBodyFrame.HasFace ?? true) || !framePosition.ToLower().Contains("back")))
                        {
                            string requiredSlots = (smapping.Ordering.FirstOrDefault(c => c.Item1 == currentFrame.Position)?.Item2 ?? "");
                            string[] attemptSlots = (new string[requiredSlots.Length / 2]).Select((c, i) => requiredSlots.Substring(i * 2, 2)).ToArray();
                            foreach (string slot in attemptSlots)
                                if (!boundLayers.ContainsKey(slot))
                                    boundLayers.Add(slot, currentEquip);
                                else
                                    boundLayers[slot] = currentEquip;
                        }
                    }
                }
            }

            foreach (Tuple<string, IEnumerable<Tuple<Equip, string, IFrame>>> eqp in zmapping.Ordering
                .Where(c => (EntireBodyFrame.HasFace ?? true) || c != "face")
                .Select(c => new Tuple<string, IEnumerable<Tuple<Equip, string, IFrame>>>(c, EquipFrames.Where(b => b.Item3.Position == c)))
                .Where(c => c.Item2 != null))
            {
                string currentZ = eqp.Item1;
                foreach (Tuple<Equip, string, IFrame> underlyingFrame in eqp.Item2)
                {
                    Equip currentEquip = underlyingFrame.Item1;
                    string framePosition = underlyingFrame.Item2;
                    IFrame currentFrame = underlyingFrame.Item3;

                    bool shouldUseEquipVSlot = framePosition == "default";

                    string requiredSlots = shouldUseEquipVSlot ? currentEquip.MetaInfo.Equip.vslot : (smapping.Ordering.FirstOrDefault(c => c.Item1 == currentZ)?.Item2 ?? "");
                    string[] attemptSlots = (new string[requiredSlots.Length / 2]).Select((c, i) => requiredSlots.Substring(i * 2, 2)).ToArray();
                    foreach (string slot in attemptSlots) if (!boundLayers.ContainsKey(slot)) boundLayers.Add(slot, currentEquip);
                    requiredLayers.Add(new Tuple<Equip, IFrame, string[]>(currentEquip, currentFrame, attemptSlots));

                    if (framePosition != currentZ && !shouldUseEquipVSlot)
                    {
                        string requiredSlotsFrame = (smapping.Ordering.FirstOrDefault(c => c.Item1 == framePosition)?.Item2 ?? "");
                        string[] attemptSlotsFrame = (new string[requiredSlotsFrame.Length / 2]).Select((c, i) => requiredSlotsFrame.Substring(i * 2, 2)).ToArray();
                        foreach (string slot in attemptSlotsFrame) if (!boundLayers.ContainsKey(slot)) boundLayers.Add(slot, currentEquip);
                        requiredLayers.Add(new Tuple<Equip, IFrame, string[]>(currentEquip, currentFrame, attemptSlotsFrame));
                    }
                }
            }

            return BodyParts.Values
                .Where(c => ShowEars || c.Name != "ear")
                .Select(c => (IFrame)c)
                .Concat(requiredLayers.Where(c => c.Item3.All(slot => boundLayers[slot] == c.Item1)).Select(c => c.Item2).DistinctBy(c => c.Position));
            }

        public Bitmap Render(ZMap zmapping, SMap smapping)
        {
            List<Tuple<string, Point, IFrame>> elements = GetElementPieces(zmapping, smapping);
            List<Tuple<int, Point, IFrame>> effectFrames = GetElementPieces(zmapping, smapping, EffectFrames.ToList())
                .Select(c => new Tuple<int, Point, IFrame>(int.Parse(c.Item1), c.Item2, c.Item3))
                .OrderBy(c => c.Item1).ToList();

            int minX = elements
                .Select(c => c.Item2.X)
                .Concat(effectFrames.Select(c => c.Item2.X))
                .Min();
            int maxX = elements
                .Select(c => c.Item2.X + c.Item3.Image.Width)
                .Concat(effectFrames.Select(c => c.Item2.X + c.Item3.Image.Width))
                .Max();
            int minY = elements
                .Select(c => c.Item2.Y)
                .Concat(effectFrames.Select(c => c.Item2.Y))
                .Min();
            int maxY = elements
                .Select(c => c.Item2.Y + c.Item3.Image.Height)
                .Concat(effectFrames.Select(c => c.Item2.Y + c.Item3.Image.Height))
                .Max();
            Size center = new Size((maxX - minX) / 2, (maxY - minY) / 2);

            Bitmap destination = new Bitmap((maxX - minX) + (Padding * 2), (maxY - minY) + (Padding * 2), PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(destination))
            {
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                foreach(Tuple<int, Point, IFrame> frame in effectFrames.Where(c => c.Item1 < 1))
                    g.DrawImage(frame.Item3.Image, new Point((frame.Item2.X - minX) + Padding, (frame.Item2.Y - minY) + Padding));
                foreach (IEnumerable<Tuple<string, Point, IFrame>> elementGroup in zmapping.Ordering.Select(c => elements.Where(i => i.Item1 == c)))
                    foreach (Tuple<string, Point, IFrame> element in elementGroup)
                        g.DrawImage(element.Item3.Image, new Point((element.Item2.X - minX) + Padding, (element.Item2.Y - minY) + Padding));
                foreach (Tuple<int, Point, IFrame> frame in effectFrames.Where(c => c.Item1 > 0))
                    g.DrawImage(frame.Item3.Image, new Point((frame.Item2.X - minX) + Padding, (frame.Item2.Y - minY) + Padding));
                g.Flush();
            }

            return destination;
        }
    }
}
