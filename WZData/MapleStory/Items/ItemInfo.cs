using reWZ;
using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WZData.ItemMetaInfo;

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

        public static ItemInfo Parse(WZDirectory source, WZObject info)
        {
            ItemInfo results = new ItemInfo();
            if (info.HasChild("only"))
                results.only = info["only"].ValueOrDefault<bool>(false);

            results.Equip = EquipInfo.Parse(info);
            results.Cash = CashInfo.Parse(info);
            results.Shop = ShopInfo.Parse(info);
            results.Card = CardInfo.Parse(info);
            results.Slot = SlotInfo.Parse(info);
            results.Chair = ChairInfo.Parse(info);
            results.Icon = IconInfo.Parse(source, info);

            return results;
        }
    }
}
