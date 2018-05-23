using Microsoft.AspNetCore.Mvc;
using PKG1;

namespace maplestory.io.Controllers
{
    [Route("api/etc/tips")]
    public class TipsController : APIController
    {
        public TipsController()
        {
            Region = Region.GMS;
            Version = "latest";
        }

        [Route("")]
        [HttpGet]
        public IActionResult GetTips() => Json(TipFactory.GetTips());
    }
}