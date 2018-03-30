using maplestory.io.Services.Interfaces.MapleStory;
using Microsoft.AspNetCore.Mvc;
using PKG1;
using maplestory.io.Data;

namespace maplestory.io.Controllers.API
{
    [Route("api/android")]
    public class AndroidController : APIController
    {
        [Route("")]
        [HttpGet]
        public IActionResult GetListing() => Json(AndroidFactory.GetAndroidIDs());

        [Route("{androidId}")]
        [HttpGet]
        public IActionResult GetAndroid(int androidId) => Json(AndroidFactory.GetAndroid(androidId));
    }
}