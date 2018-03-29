using Microsoft.AspNetCore.Mvc;
using PKG1;
using System.Collections.Generic;
using System.Linq;
using maplestory.io.Data;

namespace maplestory.io.Controllers.API
{
    [Produces("application/json")]
    [Route("api/{region}/{version}/job")]
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
    public class JobController : APIController
    {

        [Route("")]
        [HttpGet]
        [ProducesResponseType(typeof(Job[]), 200)]
        public IActionResult GetJobs() => Json(SkillFactory.GetJobs());

        [Route("{jobId}")]
        [HttpGet]
        [ProducesResponseType(typeof(Job), 200)]
        public IActionResult GetJob(int jobId)
        {
            Job job = SkillFactory.GetJob(jobId);
            if (job == null) return NotFound();
            return Json(job);
        }

        [Route("{jobId}/skillbook")]
        [HttpGet]
        [ProducesResponseType(typeof(SkillBook), 200)]
        public IActionResult GetSkillbook(int jobId)
        {
            SkillBook book = SkillFactory.GetSkillBook(jobId);
            if (book == null) return NotFound();
            return Json(book);
        }

        [Route("{jobId}/skillbook/{skillId}")]
        [HttpGet]
        [ProducesResponseType(typeof(Skill), 200)]
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
        [ProducesResponseType(typeof(SkillDescription), 200)]
        public IActionResult GetSkill(int skillId)
        {
            Skill desc = SkillFactory.GetSkill(skillId);
            if (desc == null) return NotFound();
            return Json(desc);
        }

        [Route("skilltree")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SkillTree>), 200)]
        public IActionResult GetSkillTree()
            => Json(SkillFactory.GetSkills());
    }
}