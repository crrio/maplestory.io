using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;
using maplestory.io.Data;

namespace maplestory.io.Services.Interfaces.MapleStory
{
    public interface ISkillFactory
    {
        IEnumerable<Job> GetJobs();
        Job GetJob(int id);
        SkillDescription GetSkillDescription(int id);
        SkillBook GetSkillBook(int id);
        Skill GetSkill(int id);
        IEnumerable<SkillTree> GetSkills();
    }
}
