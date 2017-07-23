using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using maplestory.io.Services.MapleStory;
using WZData;
using System.Diagnostics;
using PKG1;

namespace maplestory.io.Controllers
{
    [Produces("application/json")]
    [Route("api/{region}/{version}/job")]
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
    public class JobController : Controller
    {
        [FromRoute]
        public Region region { get; set; }
        [FromRoute]
        public string version { get; set; }
        ISkillFactory _factory;
        public JobController(ISkillFactory factory)
        {
            _factory = factory;
        }

        [Route("")]
        [HttpGet]
        [ProducesResponseType(typeof(Job[]), 200)]
        public IActionResult GetJobs() => Json(_factory.GetWithWZ(region, version).GetJobs());

        [Route("{jobId}")]
        [HttpGet]
        [ProducesResponseType(typeof(Job), 200)]
        public IActionResult GetJob(int jobId)
        {
            Job job = _factory.GetWithWZ(region, version).GetJob(jobId);
            if (job == null) return NotFound();
            return Json(job);
        }

        [Route("{jobId}/skillbook")]
        [HttpGet]
        [ProducesResponseType(typeof(SkillBook), 200)]
        public IActionResult GetSkillbook(int jobId)
        {
            SkillBook book = _factory.GetWithWZ(region, version).GetSkillBook(jobId);
            if (book == null) return NotFound();
            return Json(book);
        }

        [Route("{jobId}/skillbook/{skillId}")]
        [HttpGet]
        [ProducesResponseType(typeof(Skill), 200)]
        public IActionResult GetSkillFromBook(int jobId, int skillId)
        {
            SkillBook book = _factory.GetWithWZ(region, version).GetSkillBook(jobId);
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
            SkillDescription desc = _factory.GetWithWZ(region, version).GetSkillDescription(skillId);
            if (desc == null) return NotFound();
            return Json(desc);
        }
    }
}