using maplestory.io.Services.MapleStory;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PKG1;
using System.Linq;
using WZData;
using WZData.MapleStory.Items;

namespace maplestory.io.Controllers
{
    [Produces("application/json")]
    [Route("api/{region}/{version}/pet")]
    public class PetController : Controller
    {
        [FromRoute]
        public Region region { get; set; }
        [FromRoute]
        public string version { get; set; }

        private readonly IPetFactory _factory;
        private readonly JsonSerializerSettings serializerSettings;

        public PetController(IPetFactory factory)
        {
            _factory = factory;
        }

        [Route("{petId}")]
        [HttpGet]
        [ProducesResponseType(typeof(MapleItem), 200)]
        public IActionResult GetPet(int petId)
        {
            Pet eq = _factory.GetWithWZ(region, version).GetPet(petId);
            return Json(eq);
        }

        [Route("actions/{petId}")]
        [HttpGet]
        [ProducesResponseType(typeof(string[]), 200)]
        public IActionResult GetPetActions(int petId)
        {
            Pet eq = _factory.GetWithWZ(region, version).GetPet(petId);
            return Json(eq.frameBooks.ToDictionary(c => c.Key, c => c.Value.FirstOrDefault()?.frames?.Count() ?? 0));
        }

        [Route("{petId}/{animation}/{frame?}/{petEquip?}")]
        [HttpGet]
        [ProducesResponseType(typeof(MapleItem), 200)]
        public IActionResult RenderPet(int petId, string animation = "stand0", int frame = 0, int petEquip = -1)
        {
            return File(_factory.GetWithWZ(region, version).RenderPet(petId, animation, frame, petEquip).ImageToByte(Request), "image/png");
        }
    }
}