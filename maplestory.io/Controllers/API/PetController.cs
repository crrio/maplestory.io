using maplestory.io.Data.Items;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace maplestory.io.Controllers.API
{
    [Route("api/{region}/{version}/pet")]
    public class PetController : APIController
    {
        [Route("")]
        [HttpGet]
        public IActionResult GetAllPets() => Json(PetFactory.GetPets());

        [Route("{petId}")]
        [HttpGet]
        public IActionResult GetPet(int petId) => Json(PetFactory.GetPet(petId));

        [Route("actions/{petId}")]
        [HttpGet]
        public IActionResult GetPetActions(int petId)
        {
            Pet eq = PetFactory.GetPet(petId);
            return Json(eq.frameBooks.Where(c => c.Value.FirstOrDefault()?.frames?.Count() > 0).ToDictionary(c => c.Key, c => c.Value.FirstOrDefault()?.frames?.Count() ?? 0));
        }

        [Route("actions/{petId}/name")]
        [HttpGet]
        public IActionResult GetName(int petId)
        {
            Pet eq = PetFactory.GetPet(petId);
            return Json(eq.Description);
        }

        [Route("{petId}/{animation}/{frame?}/{petEquip?}")]
        [HttpGet]
        public IActionResult RenderPet(int petId, string animation = "stand0", int frame = 0, int petEquip = -1)
            => File(PetFactory.RenderPet(petId, animation, frame, petEquip).ImageToByte(Request), "image/png");
    }
}