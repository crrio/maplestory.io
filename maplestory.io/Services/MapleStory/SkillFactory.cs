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
        public SkillFactory(IWZFactory factory) : base(factory) { }
        public SkillFactory(IWZFactory factory, Region region, string version) : base(factory, region, version) { }

        public Job GetJob(int id) => GetJobs().Where(j => j.Id == id).FirstOrDefault();
        public IEnumerable<Job> GetJobs()
            => _factory.GetWZ(region, version).Resolve("Etc/GrowHelp/career").Children.Values.Select(c => new Job(c));
        public Skill GetSkill(int id)
        {
            WZProperty skillBooks = wz.Resolve("Skill");
            string friendlyId = id.ToString();
            WZProperty skillBook = null;
            if (skillBook == null && skillBooks.Children.ContainsKey(friendlyId.Substring(0, 6)))
                skillBook = skillBooks.Resolve($"{friendlyId.Substring(0, 6)}/skill/{friendlyId}");
            if (skillBook == null && skillBooks.Children.ContainsKey(friendlyId.Substring(0, 5)))
                skillBook = skillBooks.Resolve($"{friendlyId.Substring(0, 5)}/skill/{friendlyId}");
            if (skillBook == null && skillBooks.Children.ContainsKey(friendlyId.Substring(0, 4)))
                skillBook = skillBooks.Resolve($"{friendlyId.Substring(0, 4)}/skill/{friendlyId}");
            if (skillBook == null && skillBooks.Children.ContainsKey(friendlyId.Substring(0, 3)))
                skillBook = skillBooks.Resolve($"{friendlyId.Substring(0, 3)}/skill/{friendlyId}");
            if (skillBook == null)
                skillBook = skillBooks.Children.Values.SelectMany(c => c.Resolve("skill")?.Children?.Values).Where(c => c != null && c.Name.Equals(friendlyId)).First();

            return Skill.Parse(skillBook, GetSkillDescription);
        }
        public SkillBook GetSkillBook(int id)
            => SkillBook.Parse(wz.Resolve($"Skill/{id}"), id, GetJob(id), (c) => GetSkillDescription(c));
        public SkillDescription GetSkillDescription(int id)
            => SkillDescription.Parse(wz.Resolve($"String/Skill/{id}"));

        public IEnumerable<SkillTree> GetSkills()
            => wz.BasePackage.MainDirectory
                .ResolveOutlink("Etc/SkillSelect").Children.Values.Select(c => new SkillTree() {
                    JobId = int.Parse(c.Name),
                    Description = SkillDescription.Parse(c.ResolveOutlink($"String/Skill/{c.Name}")),
                    Skills = c.Children.Values
                        .Select(a => a.ResolveFor<int>())
                        .Where(a => a != null)
                        .Select(a => a.Value)
                        .Select(a => SkillDescription.Parse(c.ResolveOutlink($"String/Skill/{a}"))).ToArray()
                });

        public override ISkillFactory GetWithWZ(Region region, string version)
            => new SkillFactory(_factory, region, version);
    }
}
