using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using maplestory.io.Services.MapleStory;
using WZData.MapleStory.Mobs;
using WZData;
using PKG1;

namespace maplestory.io.Controllers
{
    [Produces("application/json")]
    [Route("api/{region}/{version}/mob")]
    public class MobController : Controller
    {
        [FromRoute]
        public Region region { get; set; }
        [FromRoute]
        public string version { get; set; }
        private IMobFactory _factory;

        public MobController(IMobFactory factory)
            => _factory = factory;

        [Route("")]
        [HttpGet]
        [ProducesResponseType(typeof(MobInfo[]), 200)]
        public IActionResult List() => Json(_factory.GetWithWZ(region, version).GetMobs());

        [Route("{mobId}")]
        [HttpGet]
        [ProducesResponseType(typeof(Mob), 200)]
        public IActionResult GetMob(int mobId)
        {
            return Json(_factory.GetWithWZ(region, version).GetMob(mobId));
        }

        [Route("{mobId}/icon")]
        [HttpGet]
        [ProducesResponseType(200)]
        [Produces("image/png")]
        public IActionResult GetFrame(int mobId)
        {
            Mob mobData = _factory.GetWithWZ(region, version).GetMob(mobId);

            string animation = mobData.Framebooks.ContainsKey("stand") ? "stand" : mobData.Framebooks.ContainsKey("fly") ? "fly" : null;

            if (animation == null) return NotFound();

            FrameBook standing = mobData.GetFrameBook(animation).First();
            if (standing == null) return NotFound();

            Frame firstFrame = standing.frames.First();
            if (firstFrame == null || firstFrame.Image == null) return NotFound();

            return File(firstFrame.Image.ImageToByte(), "image/png");
        }

        [Route("{mobId}/render/{framebook}/{frame?}")]
        [HttpGet]
        [ProducesResponseType(200)]
        [Produces("image/png")]
        public IActionResult Render(int mobId, string framebook, int frame = 0)
        {
            Mob mobData = _factory.GetWithWZ(region, version).GetMob(mobId);

            FrameBook standing = mobData.GetFrameBook(framebook).First();
            if (standing == null) return NotFound();

            Frame firstFrame = standing.frames.ElementAt(frame % standing.frames.Count());
            if (firstFrame == null || firstFrame.Image == null) return NotFound();

            return File(firstFrame.Image.ImageToByte(), "image/png");
        }
    }
}
