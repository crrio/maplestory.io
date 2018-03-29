using maplestory.io.Data.Images;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using PKG1;

namespace maplestory.io.Data.Items
{
    public class Equip : MapleItem
    {
        public static Action<string> ErrorCallback = (s) => { };
        public const string WZFile = "Item.wz";
        public const string FolderPath = "Etc";
        public const bool IMGId = false;
        public const string StringPath = "Eqp.img/Eqp";

        public Dictionary<string, EquipFrameBook> FrameBooks;
        public Dictionary<int, Dictionary<string, EquipFrameBook>> FrameBooksPerWeaponType;
        public string EquipGroup;
        public Effects ItemEffects;

        public Equip(int id, string group) : base(id) { FrameBooks = new Dictionary<string, EquipFrameBook>(); EquipGroup = group; }

        public static Equip Parse(WZProperty itemString)
        {
            string group = itemString.Parent.NameWithoutExtension;
            int id = int.Parse(itemString.NameWithoutExtension);
            Equip item = new Equip(id, group);
            try
            {
                WZProperty characterItem = itemString.ResolveOutlink(Path.Combine("Character", group, $"{id.ToString("D8")}.img"));
                item.MetaInfo = ItemInfo.Parse(characterItem);
                item.Description = ItemDescription.Parse(itemString, id);

                bool hasEffectsPerItemType = characterItem.Children.Where(c => c.NameWithoutExtension != "info").All(c => int.TryParse(c.NameWithoutExtension, out int blah));

                if (hasEffectsPerItemType)
                {
                    item.FrameBooksPerWeaponType = characterItem.Children.Where(c => c.NameWithoutExtension != "info")
                        .ToDictionary(c => int.Parse(c.NameWithoutExtension), c => ProcessFrameBooks(c));
                    item.FrameBooks = item.FrameBooksPerWeaponType.Values.FirstOrDefault() ?? new Dictionary<string, EquipFrameBook>();
                }
                else
                    item.FrameBooks = ProcessFrameBooks(characterItem);

                WZProperty effect = itemString.ResolveOutlink($"Effect/ItemEff/{id}");
                if (effect != null)
                    item.ItemEffects = Effects.Parse(effect);

                return item;
            }
            catch (Exception ex)
            {
                ErrorCallback($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return null;
            }
        }

        public Dictionary<string, EquipFrameBook> GetFrameBooks(int weaponType) =>
            weaponType == -100 || FrameBooksPerWeaponType == null || FrameBooksPerWeaponType.Count == 0 ? FrameBooks : FrameBooksPerWeaponType[weaponType];

        public static Dictionary<string, EquipFrameBook> ProcessFrameBooks(WZProperty container)
        {
            bool isOnlyDefault = container.Children.Where(c => c.NameWithoutExtension != "info")
                .Any(obj => obj.Type == PropertyType.Canvas || int.TryParse(obj.NameWithoutExtension, out int frameTest));

            if (isOnlyDefault)
                return new Dictionary<string, EquipFrameBook>()
                {
                    { "default", EquipFrameBook.Parse(container) }
                };
            else
                return container.Children.Where(c => c.NameWithoutExtension != "info").ToDictionary(c => c.NameWithoutExtension, obj => EquipFrameBook.Parse(obj));
        }

        public override string ToString()
            => $"{EquipGroup} - {id}";
    }

    public class Effects
    {
        public Dictionary<string, IEnumerable<FrameBook>> entries;

        readonly static string[] blacklistEntries = new []{
            "action",
            "actionExceptRotation",
            "fixed",
            "z"
        };

        public static Effects Parse(WZProperty effectContainer)
        {
            Effects effects = new Effects();

            int? z = effectContainer.ResolveFor<int>("effect/z");//itemEffect["effect"].HasChild("z") ? itemEffect["effect"]["z"].ValueOrDefault<int>(0) : 0;

            effects.entries = effectContainer.Children
                .Where(c => !blacklistEntries.Contains(c.NameWithoutExtension))
                .ToDictionary(c => c.NameWithoutExtension, c => FrameBook.Parse(c));

            return effects;
        }
    }
}
