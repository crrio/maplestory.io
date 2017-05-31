using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using maplestory.io.Services.MapleStory;
using WZData.MapleStory.NPC;
using WZData;
using System.Drawing;

namespace maplestory.io.Controllers
{
    [Produces("application/json")]
    [Route("api/npc")]
    public class NPCController : Controller
    {
        private INPCFactory _factory;

        public NPCController(INPCFactory factory)
        {
            _factory = factory;
        }

        [Route("")]
        [HttpGet]
        [ProducesResponseType(typeof(NPC[]), 200)]
        public IActionResult List() => Json(_factory.GetNPCs());

        [Route("{npcId}")]
        [HttpGet]
        [ProducesResponseType(typeof(NPC), 200)]
        public IActionResult GetNPC(int npcId)
        {
            return Json(_factory.GetNPC(npcId));
        }

        [Route("{npcId}/icon")]
        [HttpGet]
        [Produces("image/png")]
        public IActionResult GetFrame(int npcId)
        {
            NPC npcData = _factory.GetNPC(npcId);
            if (!npcData.Framebooks.ContainsKey("stand")) return NotFound();

            FrameBook standing = npcData.Framebooks["stand"].First();
            if (standing == null) return NotFound();

            Frame firstFrame = standing.frames.First();
            if (firstFrame == null || firstFrame.image == null) return NotFound();

            return File(firstFrame.image.ImageToByte(), "image/png");
        }
    }
}