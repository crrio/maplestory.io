using maplestory.io.Data.Items;
using maplestory.io.Data.Mobs;
using PKG1;
using System.Collections.Generic;
using System.Linq;

namespace maplestory.io.Data
{
    public class ItemInfo
    {
        /// <summary>
        /// Is a one-of-a-kind item
        /// </summary>
        public bool only;

        public EquipInfo Equip;
        public CashInfo Cash;
        public ShopInfo Shop;
        public CardInfo Card;
        public SlotInfo Slot;
        public ChairInfo Chair;
        public IconInfo Icon;
        public MobInfo[] DroppedBy;
        public ItemSet Set;
        public Dictionary<string, string> ConsumeSpec;

        public  static ItemInfo Parse(WZProperty characterItem)
        {
            WZProperty info = characterItem.Resolve("info");
            ItemInfo results = new ItemInfo();
            results.only = (info.ResolveFor<int>("only") ?? 0) == 1;
            results.Equip = EquipInfo.Parse(info);
            results.Cash = CashInfo.Parse(info);
            results.Shop = ShopInfo.Parse(info);
            results.Card = CardInfo.Parse(info);
            results.Slot = SlotInfo.Parse(info);
            results.Chair = ChairInfo.Parse(info);
            results.Icon = IconInfo.Parse(info);
            results.Set = ItemSet.ParseItemInfo(info);
            results.ConsumeSpec = characterItem?.Resolve("spec")?.Children.ToDictionary(c => c.NameWithoutExtension, c => c.ResolveForOrNull<string>());

            return results;
        }
    }
}
