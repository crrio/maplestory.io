using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using maplestory.io.Services.MapleStory;

namespace maplestory.io.Controllers
{
    [Produces("application/json")]
    [Route("api/Character")]
    public class CharacterController : Controller
    {
        private ICharacterFactory _factory;

        public CharacterController(ICharacterFactory factory)
        {
            _factory = factory;
        }

        [Route("base/{skinId?}")]
        [HttpGet]
        [Produces("image/png")]
        public IActionResult GetBase(int skinId = 2000)
            => File(_factory.GetBase(skinId).ImageToByte(), "image/png");
    }
}