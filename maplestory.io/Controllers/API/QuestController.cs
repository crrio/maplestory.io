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
        public IActionResult GetQuest(int questId)
        {
            var quest = QuestFactory.GetQuest(questId);
            if (quest == null) return NotFound();
            return Json(quest);
        }

        [Route("{questId}/name")]
        [HttpGet]
        public IActionResult GetName(int questId)
        {
            var questData = QuestFactory.GetQuest(questId);
            if (questData == null) return NotFound();
            return Json(new
            {
                id = questData.Id,
                name = questData.Name
            });
        }
    }
}