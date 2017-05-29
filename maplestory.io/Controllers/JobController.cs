using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using maplestory.io.Services.MapleStory;
using WZData;
using System.Diagnostics;

namespace maplestory.io.Controllers
{
    [Produces("application/json")]
    [Route("api/job")]
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
    public class JobController : Controller
    {
        ISkillFactory _factory;
        public JobController(ISkillFactory factory)
        {
            _factory = factory;
        }
        [Route("")]
        public IActionResult GetJobs() => Json(_factory.GetJobs());
        [Route("{jobId}")]
        public IActionResult GetJob(int jobId)
        {
            Job job = _factory.GetJob(jobId);
            if (job == null) return NotFound();
            return Json(job);
        }
        [Route("{jobId}/skillbook")]
        public IActionResult GetSkillbook(int jobId)
        {
            SkillBook book = _factory.GetSkillBook(jobId);
            if (book == null) return NotFound();
            return Json(book);
        }
        [Route("{jobId}/skillbook/{skillId}")]
        public IActionResult GetSkillFromBook(int jobId, int skillId)
        {
            SkillBook book = _factory.GetSkillBook(jobId);
            if (book == null) return NotFound("Couldn't find skillbook");
            Skill skill = book.Skills.Where(c => c.id == skillId).FirstOrDefault();
            if (skill == null) return NotFound("Couldn't find skill in book");
            return Json(skill);
        }
        [Route("skill/{skillId}")]
        public IActionResult GetSkill(int skillId)
        {
            SkillDescription desc = _factory.GetSkillDescription(skillId);
            if (desc == null) return NotFound();
            return Json(desc);
        }
    }
}