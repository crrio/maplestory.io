using System;
using reWZ.WZProperties;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace WZData
{
    public class SkillBook
    {
        public Image Icon;
        public Job Job;

        public IEnumerable<Skill> Skills;

        public int id;
        public SkillDescription Description;

        public static SkillBook Parse(WZObject skills, WZObject skillBook, int id, Job relatedJob, List<SkillDescription> skillDescriptions)
        {
            SkillBook book = new SkillBook();

            if (skillBook.HasChild("info"))
            {
                WZObject info = skillBook.ResolvePath("info");
                book.Icon = ResolveImage(skills, info, "icon");
            }

            book.id = id;
            book.Description = skillDescriptions.FirstOrDefault(c => c.Id == id && !string.IsNullOrEmpty(c.bookName));
            book.Skills = skillBook["skill"].Select(c => Skill.Parse(skills, book, c, skillDescriptions)).ToArray();
            book.Job = relatedJob;

            return book;
        }

        static Bitmap ResolveImage(WZObject skills, WZObject skillBookInfo, string name)
        {
            if (skillBookInfo.HasChild(name))
            { 
                WZObject icon = skillBookInfo.ResolvePath(name);
                bool hasChanged = true;
                do
                {
                    hasChanged = false;
                    while (icon.HasChild("_inlink"))
                    {
                        icon = skillBookInfo.ResolvePath("../" + icon["_inlink"].ValueOrDefault<string>(""));
                        hasChanged = true;
                    }
                    while (icon.HasChild("_outlink"))
                    {
                        string outlink = icon["_outlink"].ValueOrDefault<string>("");
                        if (outlink.StartsWith("Skill/"))
                        {
                            icon = skills.ResolvePath(outlink.Substring(6));
                            hasChanged = true;
                        }
                    }
                    while (icon is WZUOLProperty)
                    {
                        name = icon.ValueOrDefault<string>("");
                        icon = skillBookInfo.ResolvePath(name);
                        hasChanged = true;
                    }
                } while (hasChanged);
                return ((WZCanvasProperty)icon).ValueOrDefault<Bitmap>(null);
            }

            return null;
        }

        public static IEnumerable<Tuple<int, Func<SkillBook>>> GetLookup(WZDirectory skillWz, List<SkillDescription> skillDescriptions, List<Job> jobs)
        {
            foreach (WZObject skillBook in skillWz)
            {
                if (skillBook.Name.Length < 5) continue;

                string bookId = skillBook.Name.Substring(0, skillBook.Name.Length - 4);

                int id = -1;
                if (!int.TryParse(bookId, out id)) continue;

                Job relatedJob = jobs.Find((job) => job.Id == id) ?? new Job();

                yield return new Tuple<int, Func<SkillBook>>(id, CreateLookup(skillWz, skillBook, id, relatedJob, skillDescriptions));
            }
        }

        private static Func<SkillBook> CreateLookup(WZDirectory skillWz, WZObject skillBook, int id, Job relatedJob, List<SkillDescription> skillDescriptions)
            => ()
            => SkillBook.Parse(skillWz, skillBook, id, relatedJob, skillDescriptions);
    }
}