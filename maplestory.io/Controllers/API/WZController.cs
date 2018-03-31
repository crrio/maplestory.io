using maplestory.io.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace maplestory.io.Controllers
{
    [Route("api/wz")]
    public class WZController : Controller
    {
        private ApplicationDbContext _ctx;

        public WZController(ApplicationDbContext dbCtx) => _ctx = dbCtx;

        [Route("")]
        [HttpGet]
        public IActionResult Index() => Json(_ctx.MapleVersions.ToArray());
    }
}
