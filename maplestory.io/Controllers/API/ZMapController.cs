using Microsoft.AspNetCore.Mvc;
using PKG1;

namespace maplestory.io.Controllers.API
{
    [Route("api/{region}/{version}/zmap")]
    public class ZMapController : APIController
    {
        [Route("")]
        [HttpGet]
        public IActionResult GetZMap() => Json(ZMapFactory.GetZMap());
    }
}