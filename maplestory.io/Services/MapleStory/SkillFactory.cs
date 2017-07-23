using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using WZData;
using PKG1;

namespace maplestory.io.Services.MapleStory
{
    public class SkillFactory : NeedWZ<ISkillFactory>, ISkillFactory
    {
        private List<SkillDescription> skillDescriptions;
        private List<Job> jobs;
        private Dictionary<int, Func<SkillBook>> SkillBookLookup;

        public SkillFactory(IWZFactory factory) : base(factory) { }
        public SkillFactory(IWZFactory factory, Region region, string version) : base(factory, region, version) { }

        public Job GetJob(int id) => GetJobs().Where(j => j.Id == id).FirstOrDefault();
        public IEnumerable<Job> GetJobs()
            => _factory.GetWZ(region, version).Resolve("Etc/GrowHelp/career").Children.Values.Select(c => new Job(c));
        public SkillBook GetSkillBook(int id)
            => SkillBook.Parse(wz.Resolve($"Skill/{id}"), id, GetJob(id), (c) => GetSkillDescription(c));
        public SkillDescription GetSkillDescription(int id)
            => SkillDescription.Parse(wz.Resolve($"String/Skill/{id}"));
        public override ISkillFactory GetWithWZ(Region region, string version)
            => new SkillFactory(_factory, region, version);
    }
}
