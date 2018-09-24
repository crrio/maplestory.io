using maplestory.io.Data.Quests;
using maplestory.io.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

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

            if (quest.RequirementToStart.NPCId.HasValue)
                quest.RequirementToStart.NPCInfo = NPCFactory.GetNPC(quest.RequirementToStart.NPCId.Value);
            if (quest.RequirementToComplete.NPCId.HasValue)
                quest.RequirementToComplete.NPCInfo = NPCFactory.GetNPC(quest.RequirementToComplete.NPCId.Value);

            if (quest == null) return NotFound();
            return Json(quest);
        }

        [Route("category")]
        [HttpGet]
        public IActionResult GetQuestCategories()
            => Json(WZ.QuestAreaLookup.Select(c => new { id = c.Key, name = WZ.QuestAreaNames.TryGetValue(c.Key, out string name) ? name : "Unknown" }).OrderBy(c => c.id));

        [Route("category/{category}")]
        [HttpGet]
        public IActionResult GetQuestInCategory(int category)
            => WZ.QuestAreaLookup.TryGetValue(category, out var inCategory) ? Json(new
            {
                id = category,
                name = ((MSPackageCollection)WZ).QuestAreaNames.TryGetValue(category, out var categoryDetails) ? categoryDetails : "Unknown",
                quests = inCategory.Where(c => c.Item2 != null).Select(c => new { id = c.Item1, name = c.Item2 }).OrderBy(c => c.id)
            }) : (IActionResult)NotFound();

        [Route("{questId}/name")]
        [HttpGet]
        public IActionResult GetName(int questId)
        {
            var questData = QuestFactory.GetQuest(questId);
            if (questData == null) return NotFound();
            return Json(new QuestName()
            {
                id = questData.Id,
                name = questData.Name
            });
        }
    }
}