using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PKG1;

namespace maplestory.io.Data.Quests
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
        public int? Meso; // money
        public QuestState State;
        public uint? MoveToMap;

        public static QuestRewards[] Parse(WZProperty data)
        {
            if (data == null) return null;
            int id = int.Parse(data.NameWithoutExtension);
            QuestRewards onStart = QuestRewards.Parse(id, data.Resolve("0"), QuestState.Start);
            QuestRewards onComplete = QuestRewards.Parse(id, data.Resolve("1"), QuestState.Complete);

            return new QuestRewards[] { onStart, onComplete };
        }

        public static QuestRewards Parse(int id, WZProperty data, QuestState state)
        {
            if (data == null) return null;

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
            result.Items = data.Resolve("item")?.Children.Select(c => ItemReward.Parse(c));
            result.Skills = data.Resolve("skill")?.Children.Select(c => SkillReward.Parse(c));
            result.Meso = (uint?)data.ResolveFor<int>("money");
            result.MoveToMap = (uint?)data.ResolveFor<int>("transferField");

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
            if (data == null) return null;

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
            if (data == null) return null;
            SkillReward result = new SkillReward();

            result.Id = data.ResolveFor<int>("id") ?? -1;
            result.RequiredJobs = data.Resolve("job")?.Children.Select(c => Convert.ToInt32(((IWZPropertyVal)c).GetValue()));
            result.MasterLevel = data.ResolveFor<int>("masterLevel") ?? -1;
            result.SkillLevel = data.ResolveFor<int>("skillLevel") ?? -1;

            return result;
        }
    }
}
