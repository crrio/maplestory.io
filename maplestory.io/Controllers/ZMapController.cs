using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using maplestory.io.Services.MapleStory;
using PKG1;

namespace maplestory.io.Controllers.Etc
{
    [Produces("application/json")]
    [Route("api/{region}/{version}/zmap")]
    public class ZMapController : Controller
    {
        [FromRoute]
        public Region region { get; set; }
        [FromRoute]
        public string version { get; set; }

        private readonly IZMapFactory _factory;

        public ZMapController(IZMapFactory factory)
        {
            _factory = factory;
        }

        [Route("")]
        [HttpGet]
        public IActionResult GetZMap() => Json(_factory.GetZMap());
    }
}