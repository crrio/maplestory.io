using RethinkDb.Driver.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RethinkDb.Driver;

namespace maplestory.io.Models.Market
{
    public class FMRoom : FMRoom<Item> { }
    public class FMRoom<K>
        where K : Item
    {
        static readonly string[] ServerNames = new string[] {
            "Scania",
            "Windia",
            "Bera",
            "Khroa",
            "MYBCKN",
        };

        public byte channel;
        public int room;
        public int server;
        public string serverName;
        public List<Store<K>> shops;
        public string id;
        public DateTime createdAt;

        public FMRoom() { }

        public static ReqlExpr getRooms(object filter)
        {
            return RethinkDB.R
                .Db("maplestory")
                .Table("rooms")
                .Filter(filter ?? new { })
                .Map((room) => new {
                    server = room.G("server"),
                    id = room.G("id"),
                    channel = room.G("channel"),
                    createdAt = room.G("createTime"),
                    room = room.G("room"),
                    shops = room.G("shops").Values().Map((shop) => Store.MapStore(shop))
                });
        }

        public static ReqlExpr findRooms(int serverId)
        {
            return getRooms(new { server = serverId });
        }

        public static ReqlExpr findRoom(int serverId, int roomId)
        {
            return getRooms(new { server = serverId, room = roomId }).Limit(1).Nth(0);
        }
    }
}
