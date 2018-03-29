using Microsoft.AspNetCore.Mvc;
using PKG1;
using maplestory.io.Data.Images;

namespace maplestory.io.Controllers.API
{
    [Produces("application/json")]
    [Route("api/crafting/effects")]
    public class CraftingEffectsController : APIController
    {
        [Route("")]
        [HttpGet]
        [ProducesResponseType(typeof(string[]), 200)]
        public IActionResult GetListing() => Json(CraftingEffectFactory.EffectList());

        [Route("{effectName}")]
        [HttpGet]
        [ProducesResponseType(typeof(FrameBook), 200)]
        public IActionResult GetEffect(string effectName) => Json(CraftingEffectFactory.GetEffect(effectName));
    }
}