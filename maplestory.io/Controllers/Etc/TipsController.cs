using maplestory.io.Services.MapleStory;
using Microsoft.AspNetCore.Mvc;
using PKG1;

namespace maplestory.io.Controllers
{
    [Produces("application/json")]
    [Route("api/etc/tips")]
    public class TipsController : Controller
    {
        [FromRoute]
        public Region region { get; set; }
        [FromRoute]
        public string version { get; set; }
        private ITipFactory _factory;
        public TipsController(ITipFactory factory)
            => _factory = factory;

        [Route("")]
        [HttpGet]
        public IActionResult GetTips() => Json(_factory.GetWithWZ(region, version).GetTips());
    }
}