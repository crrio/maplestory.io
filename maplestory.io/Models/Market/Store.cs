using RethinkDb.Driver;
using RethinkDb.Driver.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace maplestory.io.Models.Market
{
    public class Store : Store<Item> { }
    public class Store<K>
        where K : Item
    {
        public string shopName;
        public string characterName;
        public List<K> items;

        public Store() { }

        internal static object MapStore(ReqlExpr shop)
        {
            return new
            {
                characterName = shop.G("characterName"),
                shopName = shop.G("shopName"),
                items = shop.G("items")
                    .EqJoin("id", RethinkDB.R.Db("maplestory").Table("items"))
                    .Filter((item) => item.G("right").G("Description"))
                    .Map(item => Item.MapItem(item))
            };
        }
    }
}
