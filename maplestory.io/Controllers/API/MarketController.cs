using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using maplestory.io.Services.Rethink;
using RethinkDb.Driver;
using maplestory.io.Models.Market;
using RethinkDb.Driver.Net;

namespace maplestory.io.Controllers.API
{
    [Produces("application/json")]
    [Route("api/server")]
    public class MarketController : APIController
    {
        readonly IRethinkDbConnectionFactory connectionFactory;

        public MarketController(IRethinkDbConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        [Route("{serverId}/market/legacy")]
        [HttpGet]
        [ProducesResponseType(typeof(LegacyItem), 200)]
        public async Task<IActionResult> GetLegacyMarketData(int serverId)
        {
            DateTime mostRecentTimestamp = DateTime.MinValue;
            using (var con = this.connectionFactory.CreateConnection())
            using (Cursor<FMRoom<WorldItem>> cursor = await FMRoom.findRooms(serverId).RunCursorAsync<FMRoom<WorldItem>>(con))
                return Json(new object[] {
                    new {
                        fm_items = cursor.SelectMany(room =>{
                            mostRecentTimestamp = mostRecentTimestamp > room.createdAt ? mostRecentTimestamp : room.createdAt;
                            return room.shops.SelectMany(shop =>
                            {
                                return shop.items.Select(item =>
                                {
                                    WorldItem worldItem = (WorldItem)item;
                                    worldItem.characterName = shop.characterName;
                                    worldItem.shopName = shop.shopName;
                                    worldItem.room = room.room;
                                    worldItem.channel = room.channel;
                                    return worldItem;

                                }).ToArray();
                            });
                        }).ToArray().Select(c => new LegacyItem(c))
                    },
                    new
                    {
                        seconds_ago = (int)mostRecentTimestamp.Subtract(new DateTime(1970, 1, 1)).TotalSeconds
                    }
                });
        }

        [Route("{serverId}/market/itemCount")]
        [HttpGet]
        [ProducesResponseType(typeof(int), 200)]
        public async Task<IActionResult> ItemCount(int serverId)
        {
            using (var con = this.connectionFactory.CreateConnection())
                // Have to convert to string to maintain compatibility with current API results
                return Json((await Item.GetItemCount(new { server = serverId }).RunResultAsync<int>(con)).ToString());
        }

        [Route("{serverId}/market/rooms")]
        [HttpGet]
        [ProducesResponseType(typeof(FMRoom[]), 200)]
        public async Task<IActionResult> GetRooms(int serverId)
        {
            using (var con = this.connectionFactory.CreateConnection())
            using (var cursor = await FMRoom.findRooms(serverId).RunCursorAsync<FMRoom>(con))
                return Json(cursor.ToArray());
        }

        [Route("{serverId}/market/items")]
        [HttpGet]
        [ProducesResponseType(typeof(WorldItem[][]), 200)]
        public async Task<IActionResult> GetRoomItems(int serverId)
        {
            using (var con = this.connectionFactory.CreateConnection())
            using (Cursor<FMRoom<WorldItem>> cursor = await FMRoom.findRooms(serverId).RunCursorAsync<FMRoom<WorldItem>>(con))
                return Json(cursor.SelectMany(room => room.shops.Select(shop =>
                {
                    return shop.items.Select(item =>
                    {
                        WorldItem worldItem = (WorldItem)item;
                        worldItem.characterName = shop.characterName;
                        worldItem.shopName = shop.shopName;
                        worldItem.room = room.room;
                        worldItem.channel = room.channel;
                        return worldItem;

                    });
                })).ToArray());
        }

        [Route("{serverId}/market/room/{roomId}")]
        [HttpGet]
        [ProducesResponseType(typeof(FMRoom[]), 200)]
        public async Task<IActionResult> GetRoom(int serverId, int roomId)
        {
            using (var con = this.connectionFactory.CreateConnection())
                return Json(await FMRoom.findRoom(serverId, roomId).RunResultAsync<FMRoom>(con));
        }

        [Route("{serverId}/market/room/{roomId}/items")]
        [HttpGet]
        [ProducesResponseType(typeof(ShopItem[]), 200)]
        public async Task<IActionResult> GetRoomItems(int serverId, int roomId)
        {
            using (var con = this.connectionFactory.CreateConnection())
                return Json((await FMRoom.findRoom(serverId, roomId).RunResultAsync<FMRoom<ShopItem>>(con)).shops.SelectMany(c => c.items.Select(item =>
                {
                    ShopItem shopItem = (ShopItem)item;
                    shopItem.characterName = c.characterName;
                    shopItem.shopName = c.shopName;
                    return shopItem;
                })).ToArray());
        }
    }
}