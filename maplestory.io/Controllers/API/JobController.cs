using maplestory.io.Data;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace maplestory.io.Controllers.API
{
    [Route("api/{region}/{version}/job")]
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
    public class JobController : APIController
    {

        [Route("")]
        [HttpGet]
        public IActionResult GetJobs() => Json(SkillFactory.GetJobs());

        [Route("{jobId}")]
        [HttpGet]
        public IActionResult GetJob(int jobId)
        {
            Job job = SkillFactory.GetJob(jobId);
            if (job == null) return NotFound();
            return Json(job);
        }

        [Route("{jobId}/skillbook")]
        [HttpGet]
        public IActionResult GetSkillbook(int jobId)
        {
            SkillBook book = SkillFactory.GetSkillBook(jobId);
            if (book == null) return NotFound();
            return Json(book);
        }

        [Route("{jobId}/skillbook/{skillId}")]
        [HttpGet]
        public IActionResult GetSkillFromBook(int jobId, int skillId)
        {
            SkillBook book = SkillFactory.GetSkillBook(jobId);
            if (book == null) return NotFound("Couldn't find skillbook");
            Skill skill = book.Skills.Where(c => c.id == skillId).FirstOrDefault();
            if (skill == null) return NotFound("Couldn't find skill in book");
            return Json(skill);
        }

        [Route("skill/{skillId}")]
        [HttpGet]
        public IActionResult GetSkill(int skillId)
        {
            Skill desc = SkillFactory.GetSkill(skillId);
            if (desc == null) return NotFound();
            return Json(desc);
        }

        [Route("skilltree")]
        [HttpGet]
        public IActionResult GetSkillTree()
            => Json(SkillFactory.GetSkills());
    }
}