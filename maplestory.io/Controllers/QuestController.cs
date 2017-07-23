using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using maplestory.io.Services.MapleStory;
using WZData.MapleStory.Quests;
using PKG1;

namespace maplestory.io.Controllers
{
    [Produces("application/json")]
    [Route("api/{region}/{version}/quest")]
    public class QuestController : Controller
    {
        [FromRoute]
        public Region region { get; set; }
        [FromRoute]
        public string version { get; set; }
        readonly IQuestFactory _factory;

        public QuestController(IQuestFactory factory)
            => _factory = factory;

        [Route("")]
        [HttpGet]
        [ProducesResponseType(typeof(QuestMeta[]), 200)]
        public IActionResult List() => Json(_factory.GetWithWZ(region, version).GetQuests());

        [Route("{questId}")]
        [HttpGet]
        [ProducesResponseType(typeof(Quest), 200)]
        public IActionResult GetQuest(int questId)
        {
            return Json(_factory.GetWithWZ(region, version).GetQuest(questId));
        }
    }
}