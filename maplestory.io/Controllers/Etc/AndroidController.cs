using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using maplestory.io.Services.MapleStory;
using WZData;
using WZData.MapleStory;
using PKG1;

namespace maplestory.io.Controllers.Etc
{
    [Produces("application/json")]
    [Route("api/android")]
    public class AndroidController : Controller
    {
        [FromRoute]
        public Region region { get; set; }
        [FromRoute]
        public string version { get; set; }
        private readonly IAndroidFactory _factory;

        public AndroidController(IAndroidFactory factory) => _factory = factory;

        [Route("")]
        [HttpGet]
        [ProducesResponseType(typeof(int[]), 200)]
        public IActionResult GetListing() => Json(_factory.GetWithWZ(region, version).GetAndroidIDs());

        [Route("{androidId}")]
        [HttpGet]
        [ProducesResponseType(typeof(Android), 200)]
        public IActionResult GetAndroid(int androidId) => Json(_factory.GetWithWZ(region, version).GetAndroid(androidId));
    }
}