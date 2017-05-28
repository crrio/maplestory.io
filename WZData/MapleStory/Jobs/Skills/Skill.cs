using System;
using reWZ.WZProperties;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Diagnostics;

namespace WZData
{
    public class Skill
    {
        public bool affectedByCombatOrders;
        public bool hyper;
        public bool invisible;
        public int masterLevel;
        public Dictionary<string, string> properties;
        public Image Icon, IconDisabled, IconMouseOver;
        public string[] actions;
        public string elementalAttribute;
        public int[] weapons;
        public FrameBook[] affected;
        public FrameBook[] ball;
        public FrameBook[] effect;
        public FrameBook[] hit;
        public int id;
        public SkillDescription description;

        internal static Skill Parse(WZObject skills, SkillBook book, WZObject skill, List<SkillDescription> skillDescriptions)
        {
            Skill skillEntry = new Skill();

            int skillId;
            if (!int.TryParse(skill.Name, out skillId))
                return null;

            skillEntry.id = skillId;
            skillEntry.description = skillDescriptions.FirstOrDefault(c => c.Id == skillId && string.IsNullOrEmpty(c.bookName));

            if (skill.HasChild("combatOrders"))
                skillEntry.affectedByCombatOrders = skill["combatOrders"].ValueOrDefault<int>(0) != 0;
            if (skill.HasChild("masterLevel"))
                skillEntry.masterLevel = skill["masterLevel"].ValueOrDefault<int>(0);
            if (skill.HasChild("invisible"))
                skillEntry.invisible = skill["invisible"].ValueOrDefault<int>(0) != 0;
            if (skill.HasChild("hyper"))
                skillEntry.hyper = skill["hyper"].ValueOrDefault<int>(0) != 0;
            if (skill.HasChild("action"))
                skillEntry.actions = skill["action"].Select(c => c.ValueOrDefault<string>("")).ToArray();
            if (skill.HasChild("elemAttr"))
                skillEntry.elementalAttribute = skill["elemAttr"].ValueOrDefault<string>("");

            skillEntry.weapons = skill
                .Where((c) => c.Name.StartsWith("weapon", StringComparison.CurrentCultureIgnoreCase))
                .Select(c => c.ValueOrDefault<int>(-1)).ToArray();

            if (skill.HasChild("common"))
            {
                skillEntry.properties = new Dictionary<string, string>();
                foreach(WZObject property in skill.ResolvePath("common"))
                    skillEntry.properties.Add(property.Name, property.ValueOrDefault<string>(""));
            }

            skillEntry.Icon = ResolveImage(skills, skill, "icon");
            skillEntry.IconDisabled = ResolveImage(skills, skill, "iconDisabled");
            skillEntry.IconMouseOver = ResolveImage(skills, skill, "iconMouseOver");

            if (skill.HasChild("affected"))
                skillEntry.affected = FrameBook.Parse(skills, skill, skill["affected"]).ToArray();
            if (skill.HasChild("ball"))
                skillEntry.ball = FrameBook.Parse(skills, skill, skill["ball"]).ToArray();
            if (skill.HasChild("effect"))
                skillEntry.effect = FrameBook.Parse(skills, skill, skill["effect"]).ToArray();
            if (skill.HasChild("hit"))
                skillEntry.hit = FrameBook.Parse(skills, skill, skill["hit"]).ToArray();
            return skillEntry;
        }

        static Bitmap ResolveImage(WZObject skills, WZObject skill, string name)
        {
            if (skill.HasChild(name))
            {
                WZObject icon = skill.ResolvePath(name);
                bool hasChanged = false;
                do
                {
                    hasChanged = false;
                    while (icon.HasChild("_inlink"))
                    {
                        icon = icon.ResolvePath("../../../" + icon["_inlink"].ValueOrDefault<string>(""));
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
                        if(name.StartsWith("../../../"))
                        {
                            string fromOrigin = name.Substring(9);
                            string imgName = fromOrigin.Split('/')[0];
                            name = $"../../../{imgName}.img/{fromOrigin.Substring(imgName.Length + 1)}";
                        }
                        icon = skill.ResolvePath(name);
                        hasChanged = true;
                    }
                } while (hasChanged);
                return ((WZCanvasProperty)icon).ValueOrDefault<Bitmap>(null);
            }

            return null;
        }
    }
}