using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using ImageSharp;
using ImageSharp.PixelFormats;
using PKG1;

namespace WZData
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

            if (skillBook.Children.ContainsKey("info"))
                book.Icon = skillBook.ResolveForOrNull<Image<Rgba32>>("info/icon");

            book.id = id;
            book.Description = skillDescriptions(id); //skillDescriptions.FirstOrDefault(c => c.Id == id && !string.IsNullOrEmpty(c.bookName));
            book.Skills = skillBook.Resolve("skill").Children.Select(c => Skill.Parse(c.Value, skillDescriptions));
            book.Job = relatedJob;

            return book;
        }
    }
}