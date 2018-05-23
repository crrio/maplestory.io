using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using PKG1;

namespace maplestory.io.Data
{
    public class SkillBook
    {
        public Image<Rgba32> Icon;
        public Job Job;

        public IEnumerable<Skill> Skills;

        public int id;
        public SkillDescription Description;

        public static SkillBook Parse(WZProperty skillBook, int id, Job relatedJob, Func<int, SkillDescription> skillDescriptions)
        {
            SkillBook book = new SkillBook();

            if (skillBook.Children.Any(c => c.NameWithoutExtension.Equals("info")))
                book.Icon = skillBook.ResolveForOrNull<Image<Rgba32>>("info/icon");

            book.id = id;
            book.Description = skillDescriptions(id); //skillDescriptions.FirstOrDefault(c => c.Id == id && !string.IsNullOrEmpty(c.bookName));
            book.Skills = skillBook.Resolve("skill").Children.Select(c => Skill.Parse(c, skillDescriptions));
            book.Job = relatedJob;

            return book;
        }
    }
}