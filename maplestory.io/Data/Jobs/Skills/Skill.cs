using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using SixLabors.ImageSharp;
using PKG1;
using maplestory.io.Data.Images;

namespace maplestory.io.Data
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
        public Dictionary<int, int> RequiredSkillLevels;

        public Dictionary<string, string>[] LevelProperties;

        public static Skill Parse(WZProperty skill, Func<int, SkillDescription> skillDescriptions)
        {
            Skill skillEntry = new Skill();

            int skillId;
            if (!int.TryParse(skill.NameWithoutExtension, out skillId))
                return null;

            skillEntry.id = skillId;
            skillEntry.description = skillDescriptions(skillId);

            skillEntry.affectedByCombatOrders = skill.ResolveFor<bool>("combatOrders");
            skillEntry.masterLevel = skill.ResolveFor<int>("masterLevel") ?? skill.Resolve("level")?.Children.Select(c =>
            {
                if (int.TryParse(c.NameWithoutExtension, out int skillLevel)) return new Nullable<int>(skillLevel);
                else return null;
            }).Where(c => c != null).Select(c => c.Value).Max();
            skillEntry.RequiredSkillLevels = skill.Resolve("req")?.Children
                .Where(c => int.TryParse(c.NameWithoutExtension, out int blah) && c.Type == PropertyType.Int32)
                .ToDictionary(c => int.Parse(c.NameWithoutExtension), c => c.ResolveFor<int>() ?? 1);
            skillEntry.LevelProperties = skill.Resolve("level")?.Children
                .Select(c => c.Children.ToDictionary(b => b.NameWithoutExtension, b => b.ResolveForOrNull<string>()))
                .ToArray();
            skillEntry.invisible = skill.ResolveFor<bool>("invisible");
            skillEntry.hyper = skill.ResolveFor<bool>("hyper");
            skillEntry.actions = skill.Resolve("action")?.Children.Select(c => ((IWZPropertyVal)c).GetValue().ToString());
            skillEntry.elementalAttribute = skill.ResolveForOrNull<string>("elemAttr");

            skillEntry.weapons = skill.Children
                .Where((c) => c.NameWithoutExtension.StartsWith("weapon", StringComparison.CurrentCultureIgnoreCase))
                .Select(c => Convert.ToInt32(((IWZPropertyVal)c).GetValue()));

            skillEntry.properties = skill.Resolve("common")?.Children?.ToDictionary(c => c.NameWithoutExtension, c => ((IWZPropertyVal)c).GetValue()?.ToString() ?? "");

            skillEntry.Icon = skill.ResolveForOrNull<Image<Rgba32>>("icon");
            skillEntry.IconDisabled = skill.ResolveForOrNull<Image<Rgba32>>("iconDisabled");
            skillEntry.IconMouseOver = skill.ResolveForOrNull<Image<Rgba32>>("iconMouseOver");

            if (skill.Children.Any(c => c.NameWithoutExtension.Equals("affected")))
                skillEntry.affected = FrameBook.Parse(skill.Resolve("affected"));
            if (skill.Children.Any(c => c.NameWithoutExtension.Equals("ball")))
                skillEntry.ball = FrameBook.Parse(skill.Resolve("ball"));
            if (skill.Children.Any(c => c.NameWithoutExtension.Equals("effect")))
                skillEntry.effect = FrameBook.Parse(skill.Resolve("effect"));
            if (skill.Children.Any(c => c.NameWithoutExtension.Equals("hit")))
                skillEntry.hit = FrameBook.Parse(skill.Resolve("hit"));
            return skillEntry;
        }
    }
}