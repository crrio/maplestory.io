using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using maplestory.io.Services.MapleStory;
using WZData.MapleStory.NPC;
using WZData;
using ImageSharp;
using PKG1;

namespace maplestory.io.Controllers
{
    [Produces("application/json")]
    [Route("api/{region}/{version}/npc")]
    public class NPCController : Controller
    {
        [FromRoute]
        public Region region { get; set; }
        [FromRoute]
        public string version { get; set; }

        private INPCFactory _factory;

        public NPCController(INPCFactory factory)
            => _factory = factory;

        [Route("")]
        [HttpGet]
        [ProducesResponseType(typeof(NPC[]), 200)]
        public IActionResult List() => Json(_factory.GetWithWZ(region, version).GetNPCs());

        [Route("{npcId}")]
        [HttpGet]
        [ProducesResponseType(typeof(NPC), 200)]
        public IActionResult GetNPC(int npcId)
        {
            return Json(_factory.GetWithWZ(region, version).GetNPC(npcId));
        }

        [Route("{npcId}/icon")]
        [HttpGet]
        [Produces("image/png")]
        public IActionResult GetFrame(int npcId)
        {
            NPC npcData = _factory.GetWithWZ(region, version).GetNPC(npcId);
            if (!npcData.Framebooks.ContainsKey("stand")) return NotFound();

            FrameBook standing = npcData.GetFrameBook("stand").First();
            if (standing == null) return NotFound();

            Frame firstFrame = standing.frames.First();
            if (firstFrame == null || firstFrame.Image == null) return NotFound();

            return File(firstFrame.Image.ImageToByte(Request), "image/png");
        }

        [Route("{npcId}/render/{framebook}/{frame?}")]
        [HttpGet]
        [ProducesResponseType(200)]
        [Produces("image/png")]
        public IActionResult Render(int npcId, string framebook, int frame = 0)
        {
            NPC npcData = _factory.GetWithWZ(region, version).GetNPC(npcId);

            FrameBook standing = npcData.GetFrameBook(framebook).First();
            if (standing == null) return NotFound();

            Frame firstFrame = standing.frames.ElementAt(frame % standing.frames.Count());
            if (firstFrame == null || firstFrame.Image == null) return NotFound();

            return File(firstFrame.Image.ImageToByte(Request), "image/png");
        }
    }
}