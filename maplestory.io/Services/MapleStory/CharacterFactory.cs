using System;
using System.Collections.Generic;
using ImageSharp;
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
        private readonly SMap smap;

        public CharacterFactory(IWZFactory factory, IItemFactory itemFactory, IZMapFactory zMapFactory)
        {
            skins = CharacterSkin.Parse(factory.GetWZFile(WZ.Character).MainDirectory).ToDictionary(c => c.Id);
            zmap = zMapFactory.GetZMap();
            smap = zMapFactory.GetSMap();
            this.itemFactory = itemFactory;
        }

        public CharacterSkin GetSkin(int id) => skins[id];

        public int[] GetSkinIds() => skins.Keys.ToArray();

        public Image<Rgba32> GetBase(int id, string animation = null, int frame = 0, bool showEars = false, int padding = 2)
            => GetCharacter(id, animation, frame, showEars, padding, new Tuple<int, string>[0]);

        public Image<Rgba32> GetBaseWithHair(int id, string animation = null, int frame = 0, bool showEars = false, int padding = 2, int faceId = 20305, int hairId = 37831)
            => GetCharacter(id, animation, frame, showEars, padding, faceId, hairId);

        public Image<Rgba32> GetCharacter(int id, string animation = null, int frame = 0, bool showEars = false, int padding = 2, params int[] items)
            => GetCharacter(id, animation, frame, showEars, padding, items.Select(c => new Tuple<int, string>(c, animation)).ToArray());

        public Image<Rgba32> GetCharacter(int id, string animation = null, int frame = 0, bool showEars = false, int padding = 2, params Tuple<int, string>[] itemEntries)
        {
            IEnumerable<Tuple<MapleItem, string>> items = Enumerable.Select<Tuple<int, string>, Tuple<MapleItem, string>>(itemEntries, (Func<Tuple<int, string>, Tuple<MapleItem, string>>)((Tuple<int, string> c) => (Tuple<MapleItem, string>)new Tuple<MapleItem, string>((MapleItem)itemFactory.search((int)c.Item1), (string)c.Item2)));

            CharacterSkin skin = GetSkin(id);
            CharacterAvatar avatar = new CharacterAvatar(skin);
            avatar.Items = items;

            if (animation == null)
            {
                Equip weapon = avatar.Equips.Where(c => c.EquipGroup == "Weapon").FirstOrDefault();
                animation = weapon?.FrameBooks.Select(c => c.Key).Where(c => c.Contains("stand")).FirstOrDefault() ?? "stand1";
            }

            avatar.AnimationName = animation;
            avatar.Frame = frame;
            avatar.ShowEars = showEars;
            avatar.Padding = padding;

            return avatar.Render(zmap, smap);
        }

        public Image<Rgba32> GetCompactCharacter(int skinId, string animation = null, int frame = 0, bool showEars = false, int padding = 2, params Tuple<int, string>[] itemEntries)
        {
            IEnumerable<Tuple<MapleItem, string>> items = Enumerable.Select<Tuple<int, string>, Tuple<MapleItem, string>>(itemEntries, (Func<Tuple<int, string>, Tuple<MapleItem, string>>)((Tuple<int, string> c) => (Tuple<MapleItem, string>)new Tuple<MapleItem, string>((MapleItem)itemFactory.search((int)c.Item1), (string)c.Item2)));

            CharacterSkin skin = GetSkin(skinId);
            CharacterAvatar avatar = new CharacterAvatar(skin);
            avatar.Items = items;

            if (animation == null)
            {
                Equip weapon = avatar.Equips.Where(c => c.EquipGroup == "Weapon").FirstOrDefault();
                animation = weapon?.FrameBooks.Select(c => c.Key).Where(c => c.Contains("stand")).FirstOrDefault() ?? "stand1";
            }

            avatar.AnimationName = animation;
            avatar.Frame = frame;
            avatar.ShowEars = showEars;
            avatar.Padding = padding;

            return avatar.RenderCompact(zmap, smap);
        }

        public Image<Rgba32> GetCenteredCharacter(int skinId, string animation = null, int frame = 0, bool showEars = false, int padding = 2, params Tuple<int, string>[] itemEntries)
        {
            IEnumerable<Tuple<MapleItem, string>> items = Enumerable.Select<Tuple<int, string>, Tuple<MapleItem, string>>(itemEntries, (Func<Tuple<int, string>, Tuple<MapleItem, string>>)((Tuple<int, string> c) => (Tuple<MapleItem, string>)new Tuple<MapleItem, string>((MapleItem)itemFactory.search((int)c.Item1), (string)c.Item2)));

            CharacterSkin skin = GetSkin(skinId);
            CharacterAvatar avatar = new CharacterAvatar(skin);
            avatar.Items = items;

            if (animation == null)
            {
                Equip weapon = avatar.Equips.Where(c => c.EquipGroup == "Weapon").FirstOrDefault();
                animation = weapon?.FrameBooks.Select(c => c.Key).Where(c => c.Contains("stand")).FirstOrDefault() ?? "stand1";
            }

            avatar.AnimationName = animation;
            avatar.Frame = frame;
            avatar.ShowEars = showEars;
            avatar.Padding = padding;

            return avatar.RenderCenter(zmap, smap);
        }

        public string[] GetActions(params int[] itemEntries)
        {
            Equip[] eqps = itemEntries.Select(itemFactory.search)
                .Where(c => c is Equip)
                .Select(c => (Equip)c)
                .Where(c => c.FrameBooks.ContainsKey("stand1") || c.FrameBooks.ContainsKey("stand2"))
                .ToArray();

            CharacterSkin skin = GetSkin(2000);

            return skin.Animations.Where(c => c.Value.AnimationName.Equals(c.Key, StringComparison.CurrentCultureIgnoreCase)).Select(c => c.Key).Where(c => eqps.All(e => e.FrameBooks.ContainsKey(c))).ToArray();
        }
    }
}
