using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using maplestory.io.Data;
using PKG1;
using maplestory.io.Services.Interfaces.MapleStory;

namespace maplestory.io.Services.Implementations.MapleStory
{
    public class SkillFactory : NeedWZ, ISkillFactory
    {
        public Job GetJob(int id) => GetJobs().Where(j => j.Id == id).FirstOrDefault();
        public IEnumerable<Job> GetJobs()
            => WZ.Resolve("Etc/GrowHelp/career").Children.Select(c => new Job(c));
        public Skill GetSkill(int id)
        {
            WZProperty skillBooks = WZ.Resolve("Skill");
            string friendlyId = id.ToString();
            WZProperty skillBook = null;
            if (skillBook == null)
                skillBook = skillBooks.Children.FirstOrDefault(c => c.NameWithoutExtension.Equals(friendlyId.Substring(0, 6)))?.Resolve($"skill/{friendlyId}");
            if (skillBook == null)
                skillBook = skillBooks.Children.FirstOrDefault(c => c.NameWithoutExtension.Equals(friendlyId.Substring(0, 5)))?.Resolve($"{friendlyId.Substring(0, 5)}/skill/{friendlyId}");
            if (skillBook == null)
                skillBook = skillBooks.Children.FirstOrDefault(c => c.NameWithoutExtension.Equals(friendlyId.Substring(0, 4)))?.Resolve($"{friendlyId.Substring(0, 4)}/skill/{friendlyId}");
            if (skillBook == null)
                skillBook = skillBooks.Children.FirstOrDefault(c => c.NameWithoutExtension.Equals(friendlyId.Substring(0, 3)))?.Resolve($"{friendlyId.Substring(0, 3)}/skill/{friendlyId}");
            if (skillBook == null)
                skillBook = skillBooks.Children.SelectMany(c => c.Resolve("skill")?.Children).Where(c => c != null && c.NameWithoutExtension.Equals(friendlyId)).First();

            return Skill.Parse(skillBook, GetSkillDescription);
        }
        public SkillBook GetSkillBook(int id)
            => SkillBook.Parse(WZ.Resolve($"Skill/{id}"), id, GetJob(id), (c) => GetSkillDescription(c));
        public SkillDescription GetSkillDescription(int id)
            => SkillDescription.Parse(WZ.Resolve($"String/Skill/{id}"));

        public IEnumerable<SkillTree> GetSkills()
            => WZ.BasePackage.MainDirectory
                .ResolveOutlink("Etc/SkillSelect").Children.Select(c => new SkillTree() {
                    JobId = int.Parse(c.NameWithoutExtension),
                    Description = SkillDescription.Parse(c.ResolveOutlink($"String/Skill/{c.NameWithoutExtension}")),
                    Skills = c.Children
                        .Select(a => a.ResolveFor<int>())
                        .Where(a => a != null)
                        .Select(a => a.Value)
                        .Select(a => SkillDescription.Parse(c.ResolveOutlink($"String/Skill/{a}"))).ToArray()
                });
    }
}
