using maplestory.io.Services.Interfaces.MapleStory;
using Microsoft.AspNetCore.Mvc;
using PKG1;
using maplestory.io.Data;

namespace maplestory.io.Controllers.API
{
    [Produces("application/json")]
    [Route("api/android")]
    public class AndroidController : APIController
    {
        [Route("")]
        [HttpGet]
        [ProducesResponseType(typeof(int[]), 200)]
        public IActionResult GetListing() => Json(AndroidFactory.GetAndroidIDs());

        [Route("{androidId}")]
        [HttpGet]
        [ProducesResponseType(typeof(Android), 200)]
        public IActionResult GetAndroid(int androidId) => Json(AndroidFactory.GetAndroid(androidId));
    }
}