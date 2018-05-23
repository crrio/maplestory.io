using Microsoft.AspNetCore.Mvc;

namespace maplestory.io.Controllers.API
{
    [Route("api/{region}/{version}/quest")]
    public class QuestController : APIController
    {
        [Route("")]
        [HttpGet]
        public IActionResult List() => Json(QuestFactory.GetQuests());

        [Route("{questId}")]
        [HttpGet]
        public IActionResult GetQuest(int questId) => Json(QuestFactory.GetQuest(questId));
    }
}