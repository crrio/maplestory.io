using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WZData
{
    public class SkillDescription
    {
        public int Id;
        public string desc, name, shortDesc, bookName;
        public SkillDescription(int id, string name, string description, string shortDesc, string bookName)
        {
            this.Id = id;
            this.name = name;
            this.desc = description;
            this.shortDesc = shortDesc;
            this.bookName = bookName;
        }

        public static SkillDescription Parse(WZObject child)
        {
            int itemId = -1;
            if (!int.TryParse(child.Name, out itemId))
                return null;

            string bookName = "", name = "", shortDesc = "", desc = "";

            if (child.HasChild("bookName"))
                bookName = child.ResolvePath("bookName").ValueOrDefault<string>("");
            else
            {
                if (child.HasChild("name"))
                    name = child.ResolvePath("name").ValueOrDefault<string>("");
                if (child.HasChild("desc"))
                    desc = child.ResolvePath("desc").ValueOrDefault<string>("");
                if (child.HasChild("h"))
                    shortDesc = child.ResolvePath("h").ValueOrDefault<string>("");
            }


            return new SkillDescription(itemId, name, desc, shortDesc, bookName);
        }
    }
}
