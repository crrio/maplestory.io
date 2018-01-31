using maplestory.io.Services.MapleStory;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace maplestory.io.Controllers
{
    [Route("api/wz")]
    public class WZController : Controller
    {
        private IWZFactory _factory;

        public WZController(IWZFactory factory) => _factory = factory;

        [Route("")]
        [HttpGet]
        [ProducesResponseType(typeof(Dictionary<string, string[]>), 200)]
        public IActionResult Index() => Json(_factory.GetAvailableRegionsAndVersions());
    }
}
