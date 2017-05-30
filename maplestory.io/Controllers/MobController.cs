using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using maplestory.io.Services.MapleStory;
using WZData.MapleStory.Mobs;
using WZData;

namespace maplestory.io.Controllers
{
    [Produces("application/json")]
    [Route("api/mob")]
    public class MobController : Controller
    {
        private IMobFactory _factory;

        public MobController(IMobFactory factory)
        {
            _factory = factory;
        }

        [Route("")]
        public IActionResult List() => Json(_factory.GetMobs());

        [Route("{mobId}")]
        public IActionResult GetMob(int mobId)
        {
            return Json(_factory.GetMob(mobId));
        }

        [Route("{mobId}/icon")]
        public IActionResult GetFrame(int mobId)
        {
            Mob mobData = _factory.GetMob(mobId);
            if (!mobData.Framebooks.ContainsKey("stand")) return NotFound();

            FrameBook standing = mobData.Framebooks["stand"].First();
            if (standing == null) return NotFound();

            Frame firstFrame = standing.frames.First();
            if (firstFrame == null || firstFrame.image == null) return NotFound();

            return File(firstFrame.image.ImageToByte(), "image/png");
        }
    }
}
