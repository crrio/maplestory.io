using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using maplestory.io.Services.Rethink;
using RethinkDb.Driver;
using maplestory.io.Models.Server;
using RethinkDb.Driver.Net;
using ImageSharp;

namespace maplestory.io.Controllers
{
    [Produces("application/json")]
    [Route("api/server")]
    public class ServerController : Controller
    {
        readonly IRethinkDbConnectionFactory connectionFactory;

        public ServerController(IRethinkDbConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        [Route("{serverId}")]
        [HttpGet]
        [ProducesResponseType(typeof(ServerInfo), 200)]
        public async Task<IActionResult> Worlds(int serverId)
        {
            using (var con = this.connectionFactory.CreateConnection())
                return Json(await ServerInfo.GetInfo(serverId).RunResultAsync<ServerInfo>(con));
        }

        [Route("{name}/icon")]
        [Produces("image/png")]
        [HttpGet]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Worlds(string name)
        {
            using (var con = this.connectionFactory.CreateConnection())
            {
                try
                {
                    string iconBase64 = await ServerInfo.GetIcon(name.ToLower()).RunResultAsync<string>(con);
                    byte[] iconData = Convert.FromBase64String(iconBase64);

                    return File(iconData, "image/png");
                }catch (ReqlNonExistenceError)
                {
                    return NotFound("World doesn't exist");
                }
            }
        }
    }
}