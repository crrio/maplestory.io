using Microsoft.AspNetCore.Mvc;
using PKG1;

namespace maplestory.io.Controllers.API
{
    [Produces("application/json")]
    [Route("api/{region}/{version}/zmap")]
    public class ZMapController : APIController
    {
        [Route("")]
        [HttpGet]
        public IActionResult GetZMap() => Json(ZMapFactory.GetZMap());
    }
}