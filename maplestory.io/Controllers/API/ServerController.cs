using maplestory.io.Models.Server;
using maplestory.io.Services.Rethink;
using Microsoft.AspNetCore.Mvc;
using RethinkDb.Driver;
using System;
using System.Threading.Tasks;

namespace maplestory.io.Controllers
{
    [Route("api/server")]
    public class ServerController : APIController
    {
        readonly IRethinkDbConnectionFactory connectionFactory;

        public ServerController(IRethinkDbConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        [Route("{serverId}")]
        [HttpGet]
        public async Task<IActionResult> Worlds(int serverId)
        {
            using (var con = this.connectionFactory.CreateConnection())
                return Json(await ServerInfo.GetInfo(serverId).RunResultAsync<ServerInfo>(con));
        }

        [Route("{name}/icon")]
        [HttpGet]
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