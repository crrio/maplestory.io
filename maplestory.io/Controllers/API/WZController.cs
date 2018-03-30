using maplestory.io.Services.Interfaces.MapleStory;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace maplestory.io.Controllers
{
    [Route("api/wz")]
    public class WZController : APIController
    {
        [Route("")]
        [HttpGet]
        public IActionResult Index() => Json(WZFactory.GetAvailableRegionsAndVersions());
    }
}
