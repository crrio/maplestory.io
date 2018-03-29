using maplestory.io.Services.Interfaces.MapleStory;
using Microsoft.AspNetCore.Mvc;
using PKG1;
using maplestory.io.Data.Quests;

namespace maplestory.io.Controllers.API
{
    [Produces("application/json")]
    [Route("api/{region}/{version}/quest")]
    public class QuestController : APIController
    {
        [Route("")]
        [HttpGet]
        [ProducesResponseType(typeof(QuestMeta[]), 200)]
        public IActionResult List() => Json(QuestFactory.GetQuests());

        [Route("{questId}")]
        [HttpGet]
        [ProducesResponseType(typeof(Quest), 200)]
        public IActionResult GetQuest(int questId) => Json(QuestFactory.GetQuest(questId));
    }
}