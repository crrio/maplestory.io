using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using maplestory.io.Services.MapleStory;
using WZData;

namespace maplestory.io.Controllers.Etc
{
    [Produces("application/json")]
    [Route("api/crafting/effects")]
    public class CraftingEffectsController : Controller
    {
        private readonly ICraftingEffectFactory _factory;

        public CraftingEffectsController(ICraftingEffectFactory factory) => _factory = factory;

        [Route("")]
        [HttpGet]
        [ProducesResponseType(typeof(string[]), 200)]
        public IActionResult GetListing() => Json(_factory.EffectList());

        [Route("{effectName}")]
        [HttpGet]
        [ProducesResponseType(typeof(FrameBook), 200)]
        public IActionResult GetEffect(string effectName) => Json(_factory.GetEffect(effectName));
    }
}