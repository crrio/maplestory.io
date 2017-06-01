using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using maplestory.io.Services.MapleStory;

namespace maplestory.io.Controllers.Etc
{
    [Produces("application/json")]
    [Route("api/zmap")]
    public class ZMapController : Controller
    {
        private readonly IZMapFactory _factory;

        public ZMapController(IZMapFactory factory)
        {
            _factory = factory;
        }

        [Route("")]
        public IActionResult GetZMap() => Json(_factory.GetZMap());
    }
}