using maplestory.io.Data;
using maplestory.io.Entities;
using maplestory.io.Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq;

namespace maplestory.io.Controllers
{
    [Route("api/wz")]
    public class WZController : Controller
    {
        private ApplicationDbContext _ctx;
        private JsonSerializerSettings serializerSettings;

        public WZController(ApplicationDbContext dbCtx)
        {
            _ctx = dbCtx;

            IgnorableSerializerContractResolver resolver = new IgnorableSerializerContractResolver();
            resolver.Ignore<MapleVersion>(a => a.Location);
            
            serializerSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = resolver,
                Formatting = Formatting.Indented
            };

        }

        [Route("")]
        [HttpGet]
        public IActionResult Index() => Json(_ctx.MapleVersions.ToArray(), serializerSettings);
    }
}
