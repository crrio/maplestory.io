using MoreLinq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using ImageSharp;
using System.Linq;
using WZData.MapleStory.Images;
using WZData.MapleStory.Items;
using System.Numerics;

namespace WZData.MapleStory.Characters
{
    public class CharacterAvatar
    {
        public bool ShowEars;
        public string AnimationName;
        public int Frame;
        public int Padding;
        public IEnumerable<EquipEntry> Items;
        public readonly CharacterSkin BaseSkin;
        public Dictionary<string, Vector2> anchorPositions;

        public bool HasFace { get => EntireBodyFrame.HasFace ?? false; }

        public BodyAnimation SkinAnimation { get => BaseSkin.Animations[AnimationName]; }
        public Body EntireBodyFrame { get => SkinAnimation.Frames[Frame % SkinAnimation.Frames.Length]; }
        public Dictionary<string, BodyPart> BodyParts { get => EntireBodyFrame.Parts; }

        public IEnumerable<Equip> Equips { get => Items.Select(c => c.Equip); }
        public int WeaponCategory {
            get => (int)Math.Floor(
                ((Items
                    .Where(c => c.Equip.EquipGroup.Equals("weapon", StringComparison.CurrentCultureIgnoreCase) && !c.Equip.MetaInfo.Cash.cash)
                    .FirstOrDefault()?.Equip.id ?? 0
                ) - 1000000) / 10000d
            );
        }

        public IEnumerable<EquipFrameEntry> EquipFrames
        {
            get => Items
                .GroupBy(c => c.Equip.TypeInfo.SubCategory)
                .Select(c => c.FirstOrDefault(b => b.Equip.MetaInfo.Cash?.cash ?? false) ?? c.First())
                // Some equips aren't always shown, like weapons when sitting
                .SelectMany(c => c.GetFrameEntry(WeaponCategory, AnimationName));
        }

        public IEnumerable<IFrame> EffectFrames
        {
            get => Items
                .Where(c => c.Equip.ItemEffects != null && c.Equip.ItemEffects.entries != null)
                .SelectMany(c => c.GetEffectFrameEntry(AnimationName))
                .GroupBy(c => c.Equip.EquipGroup)
                .Select(c => c.FirstOrDefault(b => b.Equip.MetaInfo.Cash?.cash ?? false) ?? c.First())
                .Select(c => c.SelectedFrame);
        }

        public CharacterAvatar(CharacterSkin baseSkin)
        {
            this.BaseSkin = baseSkin;
        }

        public List<Tuple<string, Vector2, IFrame>> GetElementPieces(ZMap zmapping, SMap smapping, List<IFrame> frames = null)
        {
            if (frames == null)
                frames = GetBodyPieces(zmapping, smapping).ToList();

            List<Tuple<string, Vector2, IFrame>> elements = new List<Tuple<string, Vector2, IFrame>>();

            while (frames.Count > 0)
            {
                IFrame part = frames.Where(c => c.MapOffset?.Any(b => anchorPositions.ContainsKey(b.Key)) ?? false).FirstOrDefault() ?? frames.First();

                Vector2 partOrigin = part.Origin ?? Vector2.Zero;
                Vector2 withOffset = Vector2.Zero;
                if (part.MapOffset != null)
                {
                    KeyValuePair<string, Vector2>? anchorVector2EntryTest = part.MapOffset.Where(c => anchorPositions.ContainsKey(c.Key)).FirstOrDefault();
                    KeyValuePair<string, Vector2> anchorVector2Entry;
                    Vector2 anchorVector2 = Vector2.Zero;
                    Vector2 vectorFromVector2 = Vector2.Zero;

                    if (anchorVector2EntryTest == null || string.IsNullOrEmpty(anchorVector2EntryTest.Value.Key))
                    {
                        Tuple<string, Vector2, IFrame> body = elements.FirstOrDefault(c => c.Item1.Equals("head"));
                        vectorFromVector2 = part.MapOffset.First().Value;
                        anchorVector2 = vectorFromVector2 + partOrigin + new Vector2(-11, (Math.Abs(body.Item2.Y) - partOrigin.Y + vectorFromVector2.Y));
                    }
                    else
                    {
                        anchorVector2Entry = anchorVector2EntryTest.Value;
                        anchorVector2 = anchorPositions[anchorVector2Entry.Key];
                        vectorFromVector2 = anchorVector2Entry.Value;
                    }

                    Vector2 fromAnchorVector2 = new Vector2(anchorVector2.X - vectorFromVector2.X, anchorVector2.Y - vectorFromVector2.Y);

                    foreach (KeyValuePair<string, Vector2> childAnchorVector2 in part.MapOffset.Where(c => c.Key != anchorVector2Entry.Key))
                    {
                        Vector2 resultAnchorVector2 = new Vector2(fromAnchorVector2.X + childAnchorVector2.Value.X, fromAnchorVector2.Y + childAnchorVector2.Value.Y);
                        if (!anchorPositions.ContainsKey(childAnchorVector2.Key))
                            anchorPositions.Add(childAnchorVector2.Key, resultAnchorVector2);
                        else if (anchorPositions[childAnchorVector2.Key].X != resultAnchorVector2.X || anchorPositions[childAnchorVector2.Key].Y != resultAnchorVector2.Y)
                        {
                            //throw new InvalidOperationException("Duplicate anchor Vector2, but position doesn't match up, possible state corruption?");
                        }
                    }

                    withOffset = new Vector2(fromAnchorVector2.X - partOrigin.X, fromAnchorVector2.Y - partOrigin.Y);
                }
                else
                {
                    Vector2 neckVector2 = (Vector2?)anchorPositions.FirstOrDefault(c => c.Key == "brow").Value ?? new Vector2(0, 0);
                    withOffset = new Vector2(neckVector2.X - partOrigin.X, neckVector2.Y - partOrigin.Y);
                }

                elements.Add(new Tuple<string, Vector2, IFrame>(part.Position, withOffset, part));
                frames.Remove(part);
            }

            return elements;
        }

        Dictionary<string, Equip> GetBoundLayers(EquipFrameEntry[] eqpFrames, ZMap zmapping, SMap smapping)
        {
            Dictionary<string, Equip> boundLayers = new Dictionary<string, Equip>();
            foreach (Tuple<string, IEnumerable<EquipFrameEntry>> eqp in zmapping.Ordering
                .Where(c => (EntireBodyFrame.HasFace ?? true) || c != "face")
                .Select(c => new Tuple<string, IEnumerable<EquipFrameEntry>>(c, eqpFrames.Where(b => b.Equip.MetaInfo.Equip.islots.Contains(c))))
                .Where(c => c.Item2 != null))
            {
                string currentZ = eqp.Item1;

                foreach (EquipFrameEntry underlyingFrame in eqp.Item2)
                {
                    Equip currentEquip = underlyingFrame.Equip;
                    string framePosition = underlyingFrame.SelectedFrame.Position ?? "";
                    IFrame currentFrame = underlyingFrame.SelectedFrame;

                    foreach (string explicitSlot in currentEquip.MetaInfo.Equip.vslots)
                    {
                        if (boundLayers.ContainsKey(explicitSlot))
                            boundLayers[explicitSlot] = currentEquip;
                        else
                            boundLayers.Add(explicitSlot, currentEquip);
                    }

                    bool shouldUseEquipVSlot = framePosition.Equals(currentEquip.EquipGroup, StringComparison.CurrentCultureIgnoreCase) || currentZ.Equals(currentEquip.EquipGroup, StringComparison.CurrentCultureIgnoreCase) || framePosition.StartsWith("default", StringComparison.CurrentCultureIgnoreCase);
                    if (!shouldUseEquipVSlot && (!(EntireBodyFrame.HasFace ?? true) || !framePosition.ToLower().Contains("back")))
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

            return boundLayers;
        }

        IEnumerable<IFrame> GetBodyPieces(ZMap zmapping, SMap smapping)
        {
            EquipFrameEntry[] eqpFrames = EquipFrames.ToArray();
            Dictionary<string, Equip> boundLayers = GetBoundLayers(eqpFrames, zmapping, smapping);
            List<Tuple<Equip, IFrame, string[]>> requiredLayers = new List<Tuple<Equip, IFrame, string[]>>();

            foreach (Tuple<string, IEnumerable<EquipFrameEntry>> eqp in zmapping.Ordering
                .Where(c => (EntireBodyFrame.HasFace ?? true) || c != "face")
                .Select(c => new Tuple<string, IEnumerable<EquipFrameEntry>>(c, eqpFrames.Where(b => b.SelectedFrame.Position == c)))
                .Where(c => c.Item2 != null))
            {
                string currentZ = eqp.Item1;
                foreach (EquipFrameEntry underlyingFrame in eqp.Item2)
                {
                    Equip currentEquip = underlyingFrame.Equip;
                    string framePosition = underlyingFrame.SelectedFrame.Position;
                    IFrame currentFrame = underlyingFrame.SelectedFrame;

                    bool shouldUseEquipVSlot = framePosition.Equals(currentEquip.EquipGroup, StringComparison.CurrentCultureIgnoreCase) || currentZ.Equals(currentEquip.EquipGroup, StringComparison.CurrentCultureIgnoreCase) || framePosition.StartsWith("default", StringComparison.CurrentCultureIgnoreCase);

                    string[] slotInstances = new string[]
                    {
                        // Equip vslot position
                        shouldUseEquipVSlot ? currentEquip.MetaInfo.Equip.vslot : null,
                        // Z position on frame part
                        smapping.Ordering.FirstOrDefault(c => c.Item1 == currentZ)?.Item2,
                        // Frame part position on item
                        smapping.Ordering.FirstOrDefault(c => c.Item1 == framePosition)?.Item2
                    };

                    slotInstances = slotInstances.Where(c => c != null).DefaultIfEmpty(currentEquip.MetaInfo.Equip.vslot).ToArray();

                    slotInstances.Where(c => c != null).ForEach(requiredSlots =>
                    {
                        string[] attemptSlots = (new string[requiredSlots.Length / 2]).Select((c, i) => requiredSlots.Substring(i * 2, 2)).ToArray();

                        foreach (string slot in attemptSlots)
                            if (!boundLayers.ContainsKey(slot))
                                boundLayers.Add(slot, currentEquip);

                        requiredLayers.Add(new Tuple<Equip, IFrame, string[]>(currentEquip, currentFrame, attemptSlots));
                    });
                }
            }

            return BodyParts.Values
                .Where(c => ShowEars || c.Name != "ear")
                .Select(c => (IFrame)c)
                .Concat(requiredLayers.Where(c => c.Item3.All(slot => boundLayers[slot] == c.Item1)).Select(c => c.Item2).DistinctBy(c => c.Position))
                .Concat(eqpFrames.Where(c => int.TryParse(c.Position, out int blah)).Select(c => c.SelectedFrame));
        }

        public Image<Rgba32> Render(ZMap zmapping, SMap smapping, string renderMode)
        {
            anchorPositions = new Dictionary<string, Vector2>() { { "navel", new Vector2(0, 0) } };
            List<Tuple<string, Vector2, IFrame>> elements = GetElementPieces(zmapping, smapping);
            List<Tuple<int, Vector2, IFrame>> effectFrames = GetElementPieces(zmapping, smapping, EffectFrames.ToList())
                .Select(c => new Tuple<int, Vector2, IFrame>(int.Parse(c.Item1), c.Item2, c.Item3))
                .Concat(elements.Where(c => int.TryParse(c.Item1, out int blah)).Select(c => new Tuple<int, Vector2, IFrame>(int.Parse(c.Item1), c.Item2, c.Item3)))
                .OrderBy(c => c.Item1).ToList();

            float minX = elements
                .Select(c => c.Item2.X)
                .Concat(effectFrames.Select(c => c.Item2.X))
                .Min();
            float maxX = elements
                .Select(c => c.Item2.X + c.Item3.Image.Width)
                .Concat(effectFrames.Select(c => c.Item2.X + c.Item3.Image.Width))
                .Max();
            float minY = elements
                .Select(c => c.Item2.Y)
                .Concat(effectFrames.Select(c => c.Item2.Y))
                .Min();
            float maxY = elements
                .Select(c => c.Item2.Y + c.Item3.Image.Height)
                .Concat(effectFrames.Select(c => c.Item2.Y + c.Item3.Image.Height))
                .Max();
            Size center = new Size((int)((maxX - minX) / 2), (int)((maxY - minY) / 2));
            Vector2 offset = new Vector2(minX, minY);

            elements = elements.Select(c => new Tuple<string, Vector2, IFrame>(c.Item1, Vector2.Subtract(c.Item2, offset), c.Item3)).ToList();
            effectFrames = effectFrames.Select(c => new Tuple<int, Vector2, IFrame>(c.Item1, Vector2.Subtract(c.Item2, offset), c.Item3)).ToList();

            Tuple<string, Vector2, IFrame> body = elements.Where(c => c.Item1.Equals("body") || c.Item1.Equals("backBody")).First();
            Vector2 bodyPosition = body.Item2;

            Image<Rgba32> destination = new Image<Rgba32>((int)((maxX - minX) + (Padding * 2)), (int)((maxY - minY) + (Padding * 2)));

            foreach (Tuple<int, Vector2, IFrame> frame in effectFrames.Where(c => c.Item1 < 1))
                destination.DrawImage(frame.Item3.Image, 1, new Size(frame.Item3.Image.Width, frame.Item3.Image.Height), new Point((int)(frame.Item2.X + Padding), (int)(frame.Item2.Y + Padding)));
            foreach (IEnumerable<Tuple<string, Vector2, IFrame>> elementGroup in zmapping.Ordering.Select(c => elements.Where(i => i.Item1 == c)))
                foreach (Tuple<string, Vector2, IFrame> element in elementGroup)
                    destination.DrawImage(element.Item3.Image, 1, new Size(element.Item3.Image.Width, element.Item3.Image.Height), new Point((int)(element.Item2.X + Padding), (int)(element.Item2.Y + Padding)));
            foreach (Tuple<int, Vector2, IFrame> frame in effectFrames.Where(c => c.Item1 > 0))
                destination.DrawImage(frame.Item3.Image, 1, new Size(frame.Item3.Image.Width, frame.Item3.Image.Height), new Point((int)(frame.Item2.X + Padding), (int)(frame.Item2.Y + Padding)));

            if (renderMode == "compact")
            {
                Vector2 bodyShouldBe = new Vector2(36, 55);
                Vector2 cropOrigin = Vector2.Subtract(bodyPosition, bodyShouldBe);
                Rectangle cropArea = new Rectangle((int)Math.Max(cropOrigin.X, 0), (int)Math.Max(cropOrigin.Y, 0), 96, 96);
                Vector2 cropOffsetFromOrigin = new Vector2(cropArea.X - cropOrigin.X, cropArea.Y - cropOrigin.Y);

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
            } else if (renderMode == "center")
            {
                Vector2 bodyCenter = Vector2.Add(body.Item2, new Vector2(body.Item3.Image.Width / 2f, 0));
                Vector2 imageCenter = new Vector2(destination.Width / 2, destination.Height / 2);
                // Positive values = body is left/above, negative = body is right/below
                Vector2 distanceFromCenter = Vector2.Multiply(2, Vector2.Subtract(imageCenter, bodyCenter));
                Image<Rgba32> centered = new Image<Rgba32>(destination.Width + (int)Math.Abs(distanceFromCenter.X), destination.Height + (int)Math.Abs(distanceFromCenter.Y));
                centered.DrawImage(destination, 1, new Size(destination.Width, destination.Height), new Point((int)Math.Max(distanceFromCenter.X, 0), (int)Math.Max(distanceFromCenter.Y, 0)));

                return centered;
            }

            return destination;
        }
    }
}
