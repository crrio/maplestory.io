using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using maplestory.io.Services.MapleStory;
using WZData;
using WZData.MapleStory;

namespace maplestory.io.Controllers.Etc
{
    [Produces("application/json")]
    [Route("api/android")]
    public class AndroidController : Controller
    {
        private readonly IAndroidFactory _factory;

        public AndroidController(IAndroidFactory factory) => _factory = factory;

        [Route("")]
        [HttpGet]
        [ProducesResponseType(typeof(int[]), 200)]
        public IActionResult GetListing() => Json(_factory.GetAndroidIDs());

        [Route("{androidId}")]
        [HttpGet]
        [ProducesResponseType(typeof(Android), 200)]
        public IActionResult GetAndroid(int androidId) => Json(_factory.GetAndroid(androidId));
    }
}