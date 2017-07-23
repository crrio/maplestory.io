using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;
using WZData;

namespace maplestory.io.Services.MapleStory
{
    public interface ISkillFactory : INeedWZ<ISkillFactory>
    {
        IEnumerable<Job> GetJobs();
        Job GetJob(int id);
        SkillDescription GetSkillDescription(int id);
        SkillBook GetSkillBook(int id);
    }
}
