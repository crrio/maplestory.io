using Microsoft.AspNetCore.Mvc;

namespace maplestory.io.Controllers.API
{
    [Route("api/{region}/{version}/quest")]
    public class QuestController : APIController
    {
        [Route("")]
        [HttpGet]
        public IActionResult List(
            [FromQuery] string searchFor = null,
            [FromQuery] int startPosition = 0,
            [FromQuery] int? count = null
        ) => Json(QuestFactory.GetQuests(searchFor, startPosition, count));

        [Route("{questId}")]
        [HttpGet]
        public IActionResult GetQuest(int questId) => Json(QuestFactory.GetQuest(questId));
    }
}