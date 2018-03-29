using Microsoft.AspNetCore.Mvc;
using PKG1;
using System.Linq;
using maplestory.io.Data.Images;
using maplestory.io.Data.Mobs;

namespace maplestory.io.Controllers.API
{
    [Produces("application/json")]
    [Route("api/{region}/{version}/mob")]
    public class MobController : APIController
    {
        [Route("")]
        [HttpGet]
        [ProducesResponseType(typeof(MobInfo[]), 200)]
        public IActionResult List() => Json(MobFactory.GetMobs());

        [Route("{mobId}")]
        [HttpGet]
        [ProducesResponseType(typeof(Mob), 200)]
        public IActionResult GetMob(int mobId)
        {
            return Json(MobFactory.GetMob(mobId));
        }

        [Route("{mobId}/icon")]
        [HttpGet]
        [ProducesResponseType(200)]
        [Produces("image/png")]
        public IActionResult GetFrame(int mobId)
        {
            Mob mobData = MobFactory.GetMob(mobId);

            string animation = mobData.Framebooks.ContainsKey("stand") ? "stand" : mobData.Framebooks.ContainsKey("fly") ? "fly" : null;

            if (animation == null) return NotFound();

            FrameBook standing = mobData.GetFrameBook(animation).First();
            if (standing == null) return NotFound();

            Frame firstFrame = standing.frames.First();
            if (firstFrame == null || firstFrame.Image == null) return NotFound();

            return File(firstFrame.Image.ImageToByte(Request), "image/png");
        }

        [Route("{mobId}/render/{framebook}/{frame?}")]
        [HttpGet]
        [ProducesResponseType(200)]
        [Produces("image/png")]
        public IActionResult Render(int mobId, string framebook, int frame = 0)
        {
            Mob mobData = MobFactory.GetMob(mobId);

            FrameBook standing = mobData.GetFrameBook(framebook).First();
            if (standing == null) return NotFound();

            Frame firstFrame = standing.frames.ElementAt(frame % standing.frames.Count());
            if (firstFrame == null || firstFrame.Image == null) return NotFound();

            return File(firstFrame.Image.ImageToByte(Request), "image/png");
        }
    }
}
