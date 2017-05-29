using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using maplestory.io.Services.MapleStory;

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
        public IActionResult List() => Json(_factory.GetNPCs());

        [Route("{npcId}")]
        public IActionResult GetNPC(int npcId)
        {
            return Json(_factory.GetNPC(npcId));
        }
    }
}
