using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using maplestory.io.Services.MapleStory;

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
    }
}
