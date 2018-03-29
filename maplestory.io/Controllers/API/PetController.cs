using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PKG1;
using System.Collections.Generic;
using System.Linq;
using maplestory.io.Data;
using maplestory.io.Data.Items;

namespace maplestory.io.Controllers.API
{
    [Produces("application/json")]
    [Route("api/{region}/{version}/pet")]
    public class PetController : APIController
    {
        [Route("")]
        [HttpGet]
        [ProducesResponseType(typeof(Dictionary<int, string>), 200)]
        public IActionResult GetAllPets() => Json(PetFactory.GetPets());

        [Route("{petId}")]
        [HttpGet]
        [ProducesResponseType(typeof(MapleItem), 200)]
        public IActionResult GetPet(int petId) => Json(PetFactory.GetPet(petId));

        [Route("actions/{petId}")]
        [HttpGet]
        [ProducesResponseType(typeof(string[]), 200)]
        public IActionResult GetPetActions(int petId)
        {
            Pet eq = PetFactory.GetPet(petId);
            return Json(eq.frameBooks.Where(c => c.Value.FirstOrDefault()?.frames?.Count() > 0).ToDictionary(c => c.Key, c => c.Value.FirstOrDefault()?.frames?.Count() ?? 0));
        }

        [Route("{petId}/{animation}/{frame?}/{petEquip?}")]
        [HttpGet]
        [ProducesResponseType(typeof(MapleItem), 200)]
        public IActionResult RenderPet(int petId, string animation = "stand0", int frame = 0, int petEquip = -1)
            => File(PetFactory.RenderPet(petId, animation, frame, petEquip).ImageToByte(Request), "image/png");
    }
}