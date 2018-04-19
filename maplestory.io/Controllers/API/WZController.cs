using maplestory.io.Data;
using maplestory.io.Entities;
using maplestory.io.Entities.Models;
using maplestory.io.Models;
using maplestory.io.Services.Interfaces.MapleStory;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PKG1;
using System.Linq;

namespace maplestory.io.Controllers
{
    [Route("api/wz")]
    public class WZController : Controller
    {
        private ApplicationDbContext _ctx;
        private readonly IWZFactory _wzFactory;
        private JsonSerializerSettings serializerSettings;

        public WZController(ApplicationDbContext dbCtx, IWZFactory wzFactory)
        {
            _ctx = dbCtx;
            _wzFactory = wzFactory;

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

        [Route("{region}/{version}/{*path}")]
        public IActionResult Query(Region region, string version, string path, [FromQuery] bool childrenOnly = false)
        {
            MSPackageCollection wz = _wzFactory.GetWZ(region, version);
            if (string.IsNullOrEmpty(path))
                return Json(wz.Packages.Keys.ToArray());

            WZProperty prop = wz.Resolve(path);
            if (prop == null) return NotFound();

            if (!childrenOnly && prop is IWZPropertyVal)
                return Json(((IWZPropertyVal)prop).GetValue());

            return Json(prop.Children.Select(c => c.Name));
        }
    }
}
