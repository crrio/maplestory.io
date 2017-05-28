using MoreLinq;
using reWZ;
using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using WZData;

namespace maplestory.io.Services.MapleStory
{
    public class SkillFactory : ISkillFactory
    {
        private List<SkillDescription> skillDescriptions;
        private List<Job> jobs;
        private Dictionary<int, Func<SkillBook>> SkillBookLookup;

        public SkillFactory(IWZFactory factory)
        {
            WZFile etcWz = factory.GetWZFile(WZ.Etc);
            jobs = etcWz.ResolvePath("GrowHelp.img/career").Select(career => new Job(career)).ToList();
            skillDescriptions = factory.GetWZFile(WZ.String).ResolvePath("Skill.img").Select(SkillDescription.Parse).Where(c => c != null).ToList();

            SkillBookLookup = new Dictionary<int, Func<SkillBook>>();
            foreach (Tuple<int, Func<SkillBook>> books in SkillBook.GetLookup(factory.GetWZFile(WZ.Skill).MainDirectory, skillDescriptions, jobs))
                SkillBookLookup.Add(books.Item1, books.Item2);
        }

        public Job GetJob(int id) => jobs.Where(j => j.Id == id).FirstOrDefault();
        public SkillBook GetSkillBook(int id) => SkillBookLookup[id]();
        public SkillDescription GetSkillDescription(int id) => skillDescriptions.Where(c => c.Id == id).FirstOrDefault();
    }
}
