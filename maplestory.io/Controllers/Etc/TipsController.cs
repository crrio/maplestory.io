using maplestory.io.Services.MapleStory;
using Microsoft.AspNetCore.Mvc;

namespace maplestory.io.Controllers
{
    [Produces("application/json")]
    [Route("api/etc/tips")]
    public class TipsController : Controller
    {
        private ITipFactory _factory;

        public TipsController(ITipFactory factory)
        {
            _factory = factory;
        }

        [Route("")]
        [HttpGet]
        public IActionResult GetTips() => Json(_factory.GetTips());
    }
}