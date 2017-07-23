using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PKG1;

namespace WZData.MapleStory.Quests
{
    public class QuestRewards
    {
        public int Id;
        public string Message; // message
        public int? Exp; // exp
        public int? BuffItemId; // buffItemId;
        public int? charmExp; // charmEXP
        public int? SenseEXP; // senseEXP
        public int? Fame; // pop
        public int? PetSkill; // petskill
        public IEnumerable<ItemReward> Items; // item
        public IEnumerable<SkillReward> Skills; // skill
        public uint? Meso; // money
        public QuestState State;

        public static QuestRewards[] Parse(WZProperty data)
        {
            int id = int.Parse(data.Name);
            QuestRewards onStart = data.Children.ContainsKey("0") && data.Resolve("0")?.Children.Count > 0 ? QuestRewards.Parse(id, data.Resolve("0"), QuestState.Start) : null;
            QuestRewards onComplete = data.Children.ContainsKey("1") && data.Resolve("1")?.Children.Count > 0 ? QuestRewards.Parse(id, data.Resolve("1"), QuestState.Complete) : null;

            return new QuestRewards[] { onStart, onComplete };
        }

        public static QuestRewards Parse(int id, WZProperty data, QuestState state)
        {
            QuestRewards result = new QuestRewards();

            result.Id = id;
            result.State = state;
            result.Message = data.ResolveForOrNull<string>("message");
            result.Exp = (int?)data.ResolveFor<int>("exp");
            result.BuffItemId = (int?)data.ResolveFor<int>("buffItemId");
            result.charmExp = (int?)data.ResolveFor<int>("charmEXP");
            result.SenseEXP = (int?)data.ResolveFor<int>("senseEXP");
            result.Fame = (int?)data.ResolveFor<int>("pop");
            result.PetSkill = (int?)data.ResolveFor<int>("petskill");
            result.Items = data.Resolve("item")?.Children.Select(c => ItemReward.Parse(c.Value));
            result.Skills = data.Resolve("skill")?.Children.Select(c => SkillReward.Parse(c.Value));
            result.Meso = (uint?)data.ResolveFor<int>("money");

            return result;
        }
    }

    public class ItemReward
    {
        public int Id; // id
        public int Count; // count
        public string PotentialGrade; // potentialGrade
        public bool? Gender; // gender
        public int? Job;

        public static ItemReward Parse(WZProperty data)
        {
            ItemReward result = new ItemReward();

            result.Id = data.ResolveFor<int>("id") ?? -1;
            result.Count = data.ResolveFor<int>("count") ?? 0;
            result.PotentialGrade = data.ResolveForOrNull<string>("potentialGrade");
            result.Gender = data.ResolveFor<bool>("gender");
            result.Job = data.ResolveFor<int>("job");

            return result;
        }
    }

    public class SkillReward
    {
        public int Id; // id
        public IEnumerable<int> RequiredJobs; // job
        public int MasterLevel; // masterLevel
        public int SkillLevel; // skillLevel

        public static SkillReward Parse(WZProperty data)
        {
            SkillReward result = new SkillReward();

            result.Id = data.ResolveFor<int>("id") ?? -1;
            result.RequiredJobs = data.Resolve("job")?.Children.Select(c => Convert.ToInt32(((IWZPropertyVal)c.Value).GetValue()));
            result.MasterLevel = data.ResolveFor<int>("masterLevel") ?? -1;
            result.SkillLevel = data.ResolveFor<int>("skillLevel") ?? -1;

            return result;
        }
    }
}
