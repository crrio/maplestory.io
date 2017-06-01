using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using WZData;
using WZData.MapleStory;
using WZData.MapleStory.Characters;
using WZData.MapleStory.Items;

namespace maplestory.io.Services.MapleStory
{
    public class CharacterFactory : ICharacterFactory
    {
        private readonly Dictionary<int, CharacterSkin> skins;
        private readonly IItemFactory itemFactory;
        private readonly ZMap zmap;

        public CharacterFactory(IWZFactory factory, IItemFactory itemFactory, IZMapFactory zMapFactory)
        {
            skins = CharacterSkin.Parse(factory.GetWZFile(WZ.Character).MainDirectory).ToDictionary(c => c.Id);
            zmap = zMapFactory.GetZMap();
            this.itemFactory = itemFactory;
        }

        public CharacterSkin GetSkin(int id) => skins[id];

        public int[] GetSkinIds() => skins.Keys.ToArray();

        public Bitmap GetBase(int id, string animation = "stand1", int frame = 0, bool showEars = false, int padding = 2)
            => GetCharacter(id, animation, frame, showEars, padding);

        public Bitmap GetBaseWithHair(int id, string animation = "stand1", int frame = 0, bool showEars = false, int padding = 2, int faceId = 20305, int hairId = 37831)
            => GetCharacter(id, animation, frame, showEars, padding, faceId, hairId);

        public Bitmap GetCharacter(int id, string animation = "stand1", int frame = 0, bool showEars = false, int padding = 2, params int[] items)
        {
            CharacterSkin skin = GetSkin(id);
            BodyAnimation bodyAnimation = skin.Animations[animation];
            Body bodyFrame = bodyAnimation.Frames[frame % bodyAnimation.Frames.Length];
            bool hasFace = bodyFrame.HasFace ?? false;
            Dictionary<string, BodyPart> bodyParts = bodyFrame.Parts;

            IEnumerable<IFrame> equipFrames = items.Select(itemFactory.search)
                .Where(c => c is Equip)
                .Select(c => (Equip)c)
                .GroupBy(c => c.MetaInfo.Equip.islot)
                .Select(c => c.FirstOrDefault(b => b.MetaInfo.Cash?.cash ?? false) ?? c.First())
                // Some equips aren't always shown, like weapons when sitting
                .Where(c => c.FrameBooks.ContainsKey(animation) || c.FrameBooks.ContainsKey("default"))
                .Select(c => c.FrameBooks.ContainsKey(animation) ? c.FrameBooks[animation] : c.FrameBooks["default"])
                .Select(c => c.frames.Count() <= frame ? c.frames.ElementAt(frame % c.frames.Count()) : c.frames.ElementAt(frame))
                .SelectMany(c => c.Effects.Values);

            Dictionary<string, IFrame> parts = bodyParts.Values
                .Where(c => showEars || c.Name != "ear")
                .Select(c => (IFrame)c)
                .Concat(bodyParts.Values.Where(c => c.Position == "body" || c.Position == "head").Select(c => new BodyPart()
                {
                    Image = c.Image,
                    MapOffset = c.MapOffset,
                    Name = c.Name.Replace("body", "Bd").Replace("head", "Hd"),
                    Origin = c.Origin,
                    Position = c.Position.Replace("body", "Bd").Replace("head", "Hd")
                }))
                .Concat(equipFrames)
                .Where(c => hasFace || c.Position != "face")
                .ToDictionary(c => c.Position);

            Dictionary<string, Point> positions = new Dictionary<string, Point>() { { "navel", new Point(0, 0) } };
            List<IFrame> characterParts = parts.Values.ToList();
            Dictionary<string, Tuple<Point, IFrame>> elements = new Dictionary<string, Tuple<Point, IFrame>>();
            bool hasCap = false;
            while (characterParts.Count > 0)
            {
                IFrame part = characterParts.Where(c => c.MapOffset.Any(b => positions.ContainsKey(b.Key))).FirstOrDefault();
                if (part == null)
                    throw new InvalidOperationException("We have body parts, but none of them have an anchor point to the navel? That doesn't seem correct.");

                KeyValuePair<string, Point> anchorPointEntry = part.MapOffset.Where(c => positions.ContainsKey(c.Key)).First();
                Point anchorPoint = positions[anchorPointEntry.Key];
                Point vectorFromPoint = anchorPointEntry.Value;
                Point fromAnchorPoint = new Point(anchorPoint.X - vectorFromPoint.X, anchorPoint.Y - vectorFromPoint.Y);
                foreach (KeyValuePair<string, Point> childAnchorPoint in part.MapOffset.Where(c => c.Key != anchorPointEntry.Key))
                {
                    Point resultAnchorPoint = new Point(fromAnchorPoint.X + childAnchorPoint.Value.X, fromAnchorPoint.Y + childAnchorPoint.Value.Y);
                    if (!positions.ContainsKey(childAnchorPoint.Key))
                        positions.Add(childAnchorPoint.Key, resultAnchorPoint);
                    else if (positions[childAnchorPoint.Key].X != resultAnchorPoint.X || positions[childAnchorPoint.Key].Y != resultAnchorPoint.Y)
                        throw new InvalidOperationException("Duplicate anchor point, but position doesn't match up, possible state corruption?");
                }
                Point partOrigin = part.Origin ?? Point.Empty;
                Point withOffset = new Point(fromAnchorPoint.X - partOrigin.X, fromAnchorPoint.Y - partOrigin.Y);

                elements.Add(part.Position, new Tuple<Point, IFrame>(withOffset, part));
                characterParts.Remove(part);
                // Too lazy to find the real toggle for not showing hairOverHead, this seems to work without issue
                hasCap = hasCap || part.Position == "cap";
            }

            if (hasCap && elements.ContainsKey("hairOverHead")) elements.Remove("hairOverHead");

            int minX = elements.Select(c => c.Value.Item1.X).Min();
            int maxX = elements.Select(c => c.Value.Item1.X + c.Value.Item2.Image.Width).Max();
            int minY = elements.Select(c => c.Value.Item1.Y).Min();
            int maxY = elements.Select(c => c.Value.Item1.Y + c.Value.Item2.Image.Height).Max();
            Size center = new Size((maxX - minX) / 2, (maxY - minY) / 2);

            Bitmap destination = new Bitmap((maxX - minX) + (padding * 2), (maxY - minY) + (padding * 2));
            using (Graphics g = Graphics.FromImage(destination))
            {
                foreach (Tuple<Point, IFrame> element in zmap.Ordering.Where(c => elements.ContainsKey(c)).Select(c => elements[c]))
                    g.DrawImage(element.Item2.Image, new Point((element.Item1.X - minX) + padding, (element.Item1.Y - minY) + padding));

                g.Flush();
            }

            return destination;
        }
    }
}
