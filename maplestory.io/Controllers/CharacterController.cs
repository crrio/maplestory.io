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
        private readonly IItemFactory _itemFactory;
        private Random rng;

        public CharacterController(ICharacterFactory factory, IItemFactory items)
        { 
            _factory = factory;
            _itemFactory = items;
            rng = new Random();
        }

        [Route("base/{skinId?}")]
        [HttpGet]
        [Produces("image/png")]
        public IActionResult GetBase(int skinId = 2000)
            => File(_factory.GetBase(skinId).ImageToByte(), "image/png");

        [Route("base/{skinId?}/example")]
        [HttpGet]
        [Produces("image/png")]
        public IActionResult GetBaseExample(int skinId = 2000)
            => File(_factory.GetBaseWithHair(skinId).ImageToByte(), "image/png");

        [Route("{skinId}/{items}/{animation?}/{frame?}")]
        [HttpGet]
        [Produces("image/png")]
        public IActionResult GetCharacter(int skinId, string items, string animation = null, int frame = 0)
            => File(_factory.GetCharacter(skinId, animation, frame, items: items
                    .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(c => int.TryParse(c, out int blah))
                    .Select(c => int.Parse(c))
                    .ToArray()
                ).ImageToByte(), "image/png");

        [Route("")]
        [HttpGet]
        [ProducesResponseType(typeof(int[]), 200)]
        public IActionResult GetSkinTypes() => Json(_factory.GetSkinIds());

        [Route("random")]
        [HttpGet]
        [Produces("image/png")]
        public IActionResult GetRandomCharacter()
        {
            byte level = (byte)rng.Next(0, byte.MaxValue);

            return File(new byte[] { }, "image/png");
        }
    }
}