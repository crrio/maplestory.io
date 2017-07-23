using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using ImageSharp;
using PKG1;

namespace WZData
{
    public class Skill
    {
        public bool? affectedByCombatOrders;
        public bool? hyper;
        public bool? invisible;
        public int? masterLevel;
        public Dictionary<string, string> properties;
        public Image<Rgba32> Icon, IconDisabled, IconMouseOver;
        public IEnumerable<string> actions;
        public string elementalAttribute;
        public IEnumerable<int> weapons;
        public IEnumerable<FrameBook> affected;
        public IEnumerable<FrameBook> ball;
        public IEnumerable<FrameBook> effect;
        public IEnumerable<FrameBook> hit;
        public int id;
        public string soundPath { get => $"Skill.img/{id}"; }
        public SkillDescription description;

        internal static Skill Parse(SkillBook book, WZProperty skill, Func<int, SkillDescription> skillDescriptions)
        {
            Skill skillEntry = new Skill();

            int skillId;
            if (!int.TryParse(skill.Name, out skillId))
                return null;

            skillEntry.id = skillId;
            skillEntry.description = skillDescriptions(skillId);

            skillEntry.affectedByCombatOrders = skill.ResolveFor<bool>("combatOrders");
            skillEntry.masterLevel = skill.ResolveFor<int>("masterLevel");
            skillEntry.invisible = skill.ResolveFor<bool>("invisible");
            skillEntry.hyper = skill.ResolveFor<bool>("hyper");
            skillEntry.actions = skill.Resolve("action").Children.Select(c => ((IWZPropertyVal)c.Value).GetValue().ToString());
            skillEntry.elementalAttribute = skill.ResolveForOrNull<string>("elemAttr");

            skillEntry.weapons = skill.Children
                .Where((c) => c.Key.StartsWith("weapon", StringComparison.CurrentCultureIgnoreCase))
                .Select(c => Convert.ToInt32(((IWZPropertyVal)c.Value).GetValue()));

            skillEntry.properties = skill.Resolve("common").Children.ToDictionary(c => c.Key, c => ((IWZPropertyVal)c.Value).GetValue().ToString());

            skillEntry.Icon = skill.ResolveForOrNull<Image<Rgba32>>("icon");
            skillEntry.IconDisabled = skill.ResolveForOrNull<Image<Rgba32>>("iconDisabled");
            skillEntry.IconMouseOver = skill.ResolveForOrNull<Image<Rgba32>>("iconMouseOver");

            if (skill.Children.ContainsKey("affected"))
                skillEntry.affected = FrameBook.Parse(skill.Resolve("affected"));
            if (skill.Children.ContainsKey("ball"))
                skillEntry.ball = FrameBook.Parse(skill.Resolve("ball"));
            if (skill.Children.ContainsKey("effect"))
                skillEntry.effect = FrameBook.Parse(skill.Resolve("effect"));
            if (skill.Children.ContainsKey("hit"))
                skillEntry.hit = FrameBook.Parse(skill.Resolve("hit"));
            return skillEntry;
        }
    }
}