using WZData.MapleStory.Images;
using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using reWZ;

namespace WZData.MapleStory.Items
{
    public class Equip : MapleItem
    {
        public const string WZFile = "Item.wz";
        public const string FolderPath = "Etc";
        public const bool IMGId = false;
        public const string StringPath = "Eqp.img/Eqp";

        public Dictionary<string, EquipFrameBook> FrameBooks;
        public Dictionary<int, Dictionary<string, EquipFrameBook>> FrameBooksPerWeaponType;
        public string EquipGroup;
        public Effects ItemEffects;

        public Equip(int id, string group) : base(id) { FrameBooks = new Dictionary<string, EquipFrameBook>(); EquipGroup = group; }

        public static Equip Parse(WZDirectory characterWz, WZObject stringItem, string group, int id, WZDirectory stringWz, WZObject itemEffectsImg)
        {
            Equip item = new Equip(id, group);
            try
            {
                WZObject characterItem = characterWz.ResolvePath(Path.Combine(group, $"{id.ToString("D8")}.img"));
                if (characterItem.HasChild("info")) item.MetaInfo = ItemInfo.Parse(characterWz, characterItem["info"]);
                item.Description = ItemDescription.Parse(stringItem, StringPath);

                bool hasEffectsPerItemType = characterItem.Where(c => c.Name != "info").All(c => int.TryParse(c.Name, out int blah));

                if (hasEffectsPerItemType)
                {
                    item.FrameBooksPerWeaponType = characterItem.Where(c => c.Name != "info")
                        .ToDictionary(c => int.Parse(c.Name), c => ProcessFrameBooks(characterWz, characterItem, c));
                    item.FrameBooks = item.FrameBooksPerWeaponType.Values.FirstOrDefault() ?? new Dictionary<string, EquipFrameBook>();
                }
                else
                    item.FrameBooks = ProcessFrameBooks(characterWz, characterItem, characterItem);

                if (itemEffectsImg.HasChild(item.id.ToString()))
                    item.ItemEffects = Effects.Parse(itemEffectsImg[item.id.ToString()], itemEffectsImg);

                return item;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Dictionary<string, EquipFrameBook> GetFrameBooks(int weaponType) => weaponType == -100 || FrameBooksPerWeaponType == null || FrameBooksPerWeaponType.Count == 0 ? FrameBooks : FrameBooksPerWeaponType[weaponType];

        public static Dictionary<string, EquipFrameBook> ProcessFrameBooks(WZObject characterWz, WZObject characterItem, WZObject container)
        {
            bool isOnlyDefault = container.Where(c => c.Name != "info").Any(obj => obj.Type == WZObjectType.Canvas || int.TryParse(obj.Name, out int frameTest));

            if (isOnlyDefault)
                return new Dictionary<string, EquipFrameBook>()
                {
                    { "default", EquipFrameBook.Parse(characterWz, characterItem, characterItem) }
                };
            else
                return container.Where(c => c.Name != "info").ToDictionary(c => c.Name, obj => EquipFrameBook.Parse(characterWz, characterItem, obj));
        }

        public static Equip Search(WZDirectory characterWz, WZDirectory effectWz, WZDirectory stringWz, int itemId)
        {
            int id = -1;
            foreach (WZObject idGrouping in stringWz.ResolvePath(StringPath))
                foreach (WZObject item in idGrouping)
                    if (int.TryParse(item.Name, out id) && id == itemId)
                        return Equip.Parse(characterWz, item, idGrouping.Name, id, stringWz, effectWz["ItemEff.img"]);
            return null;
        }

        public static IEnumerable<Tuple<int, Func<MapleItem>>> GetLookup(Func<Func<WZFile, MapleItem>, MapleItem> characterWz, Func<Func<WZFile, MapleItem>, MapleItem> effectWzCallback, WZFile stringWz)
        {
            int id = -1;
            foreach (WZObject idGrouping in stringWz.ResolvePath(StringPath))
                foreach (WZObject item in idGrouping)
                    if (int.TryParse(item.Name, out id))
                        yield return new Tuple<int, Func<MapleItem>>(id, CreateLookup(characterWz, effectWzCallback, item, idGrouping.Name, id, stringWz).Memoize());
        }

        private static Func<MapleItem> CreateLookup(Func<Func<WZFile, MapleItem>, MapleItem> characterWzCallback, Func<Func<WZFile, MapleItem>, MapleItem> effectWzCallback, WZObject item, string idGroupingName, int id, WZFile stringWz)
            => ()
            => characterWzCallback((characterWz) => effectWzCallback(effectWz => Equip.Parse(characterWz.MainDirectory, item, idGroupingName, id, stringWz.MainDirectory, effectWz.MainDirectory["ItemEff.img"])));

        public override string ToString()
            => $"{EquipGroup} - {id}";
    }

    public class Effects
    {
        public Dictionary<string, IEnumerable<FrameBook>> entries;

        public static Effects Parse(WZObject itemEffect, WZObject effectsWz)
        {
            Effects effects = new Effects();

            effects.entries = itemEffect.HasChild("effect") ? itemEffect["effect"].Where(c =>
                !c.Name.Equals("action", StringComparison.CurrentCultureIgnoreCase) &&
                !c.Name.Equals("actionExceptRotation", StringComparison.CurrentCultureIgnoreCase) &&
                !c.Name.Equals("fixed", StringComparison.CurrentCultureIgnoreCase) &&
                !c.Name.Equals("z", StringComparison.CurrentCultureIgnoreCase)
            ).ToDictionary(c => c.Name, c => FrameBook.Parse(effectsWz, itemEffect.Parent, c)) : null;

            return effects;
        }
    }
}
