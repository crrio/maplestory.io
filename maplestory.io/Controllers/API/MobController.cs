using maplestory.io.Data.Images;
using maplestory.io.Data.Mobs;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace maplestory.io.Controllers.API
{
    [Route("api/{region}/{version}/mob")]
    public class MobController : APIController
    {
        [Route("")]
        [HttpGet]
        public IActionResult List(
            [FromQuery] int startPosition = 0, 
            [FromQuery] int? count = null,
                        [FromQuery] int? minLevelFilter = null,
            [FromQuery] int? maxLevelFilter = null,
            [FromQuery] string searchFor = null
        ) => Json(MobFactory.GetMobs(startPosition, count, minLevelFilter, maxLevelFilter, searchFor));

        [Route("{mobId}")]
        [HttpGet]
        public IActionResult GetMob(int mobId)
        {
            return Json(MobFactory.GetMob(mobId));
        }

        [Route("{mobId}/icon")]
        [HttpGet]
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
