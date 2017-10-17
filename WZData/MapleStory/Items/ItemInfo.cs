using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WZData.ItemMetaInfo;
using PKG1;
using WZData.MapleStory.Mobs;
using WZData.MapleStory.Items;

namespace WZData
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
            results.ConsumeSpec = characterItem?.Resolve("spec")?.Children.ToDictionary(c => c.Key, c => c.Value.ResolveForOrNull<string>());

            return results;
        }
    }
}
