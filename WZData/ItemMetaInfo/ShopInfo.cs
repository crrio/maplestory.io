using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WZData.ItemMetaInfo
{
    public class ShopInfo
    {
        /// <summary>
        /// Sold to NPC for
        /// </summary>
        public int price;
        /// <summary>
        /// Can't be sold
        /// </summary>
        public bool notSale;
        /// <summary>
        /// Is a monster book card
        /// </summary>
        public bool monsterBook;

        public static ShopInfo Parse(WZObject info)
        {
            if (!(info.HasChild("price") || info.HasChild("notSale") || info.HasChild("monsterBook")))
                return null;

            ShopInfo results = new ShopInfo();

            if (info.HasChild("price"))
                results.price = info.ValueOrDefault<int>(0);
            if (info.HasChild("notSale"))
                results.notSale = info.ValueOrDefault<int>(0) == 1;
            if (info.HasChild("monsterBook"))
                results.monsterBook = info.ValueOrDefault<int>(0) == 1;

            return results;
        }
    }
}
