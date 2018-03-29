using Microsoft.AspNetCore.Mvc;
using System.Linq;
using maplestory.io.Data.Images;
using maplestory.io.Data.NPC;

namespace maplestory.io.Controllers
{
    [Produces("application/json")]
    [Route("api/{region}/{version}/npc")]
    public class NPCController : APIController
    {
        [Route("")]
        [HttpGet]
        [ProducesResponseType(typeof(NPC[]), 200)]
        public IActionResult List() => Json(NPCFactory.GetNPCs());

        [Route("{npcId}")]
        [HttpGet]
        [ProducesResponseType(typeof(NPC), 200)]
        public IActionResult GetNPC(int npcId)
        {
            return Json(NPCFactory.GetNPC(npcId));
        }

        [Route("{npcId}/icon")]
        [HttpGet]
        [Produces("image/png")]
        public IActionResult GetFrame(int npcId)
        {
            NPC npcData = NPCFactory.GetNPC(npcId);
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
            NPC npcData = NPCFactory.GetNPC(npcId);

            FrameBook standing = npcData.GetFrameBook(framebook).First();
            if (standing == null) return NotFound();

            Frame firstFrame = standing.frames.ElementAt(frame % standing.frames.Count());
            if (firstFrame == null || firstFrame.Image == null) return NotFound();

            return File(firstFrame.Image.ImageToByte(Request), "image/png");
        }
    }
}