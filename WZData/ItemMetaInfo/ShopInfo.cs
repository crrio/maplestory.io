using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;

namespace WZData.ItemMetaInfo
{
    public class ShopInfo
    {
        readonly static string[] mustContainOne = new []{
            "price",
            "notSale",
            "monsterBook"
        };

        /// <summary>
        /// Sold to NPC for
        /// </summary>
        public int? price;
        /// <summary>
        /// Can't be sold
        /// </summary>
        public bool? notSale;
        /// <summary>
        /// Is a monster book card
        /// </summary>
        public bool? monsterBook;

        public static ShopInfo Parse(WZProperty info)
        {
            if (!info.Children.Keys.Any(c => mustContainOne.Contains(c)))
                return null;

            ShopInfo results = new ShopInfo();

            results.price = info.ResolveFor<int>("price");
            results.notSale = info.ResolveFor<bool>("notSale");
            results.monsterBook = info.ResolveFor<bool>("monsterBook");

            return results;
        }
    }
}
