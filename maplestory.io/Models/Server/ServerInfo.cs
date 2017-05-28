using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using maplestory.io.Models.Market;
using RethinkDb.Driver;
using RethinkDb.Driver.Ast;

namespace maplestory.io.Models.Server
{
    public class ServerInfo
    {
        static readonly string[][] serverNames = new string[][]
        {
            new string[] { "scania" },
            new string[] { "windia" },
            new string[] { "bera" },
            new string[] { "khaini", "broa" },
            new string[] { "mardia", "yellonde", "bellocan", "chaos", "kradia", "nova" },
            new string[] { "galicia", "renegades", "arcania", "zenith", "elnido", "demethos" }
        };

        public long itemCount;
        public WorldInfo[] worlds;

        public static ReqlExpr GetInfo(int worldId)
        {
            return RethinkDB.R.Expr(new
            {
                itemCount = Item.GetItemCount(new { server = worldId }),
                worlds = RethinkDB.R
                        .Db("maplestory")
                        .Table("worlds")
                        .GetAll(RethinkDB.R.Args(serverNames[worldId]))
                        .CoerceTo("array")
            });
        }

        public static ReqlExpr GetIcon(string worldName)
        {
            return RethinkDB.R
                .Db("maplestory")
                .Table("worlds")
                .Get(worldName).G("Icon");
        }
    }
}
