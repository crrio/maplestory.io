﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;

namespace maplestory.io.Data
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

        public static SkillDescription Parse(WZProperty child)
        {
            if (child == null) return null;
            int itemId = -1;
            if (!int.TryParse(child.NameWithoutExtension, out itemId))
                return null;

            string bookName = "", name = "", shortDesc = "", desc = "";

            if (child.Children.Any(c => c.NameWithoutExtension.Equals("bookName")))
                bookName = child.ResolveForOrNull<string>("bookName");
            else
            {
                name = child.ResolveForOrNull<string>("name");
                desc = child.ResolveForOrNull<string>("desc");
                shortDesc = child.ResolveForOrNull<string>("h");
            }

            return new SkillDescription(itemId, name, desc, shortDesc, bookName);
        }
    }
}
