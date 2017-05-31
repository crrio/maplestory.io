using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public static QuestRewards[] Parse(WZObject data)
        {
            int id = int.Parse(data.Name);
            QuestRewards onStart = data.HasChild("0") && data["0"].ChildCount > 0 ? QuestRewards.Parse(id, data["0"], QuestState.Start) : null;
            QuestRewards onComplete = data.HasChild("1") && data["1"].ChildCount > 0 ? QuestRewards.Parse(id, data["1"], QuestState.Complete) : null;

            return new QuestRewards[] { onStart, onComplete };
        }

        public static QuestRewards Parse(int id, WZObject data, QuestState state)
        {
            QuestRewards result = new QuestRewards();

            result.Id = id;
            result.State = state;
            result.Message = data.HasChild("message") ? data["message"].ValueOrDefault<string>(null) : null;
            result.Exp = data.HasChild("exp") ? (int?)data["exp"].ValueOrDefault<int>(0) : null;
            result.BuffItemId = data.HasChild("buffItemId") ? (int?)data["buffItemId"].ValueOrDefault<int>(0) : null;
            result.charmExp = data.HasChild("charmEXP") ? (int?)data["charmEXP"].ValueOrDefault<int>(0) : null;
            result.SenseEXP = data.HasChild("senseEXP") ? (int?)data["senseEXP"].ValueOrDefault<int>(0) : null;
            result.Fame = data.HasChild("pop") ? (int?)data["pop"].ValueOrDefault<int>(0) : null;
            result.PetSkill = data.HasChild("petskill") ? (int?)data["petskill"].ValueOrDefault<int>(0) : null;
            result.Items = data.HasChild("item") ? data["item"].Select(c => ItemReward.Parse(c)) : null;
            result.Skills = data.HasChild("skill") ? data["skill"].Select(c => SkillReward.Parse(c)) : null;
            result.Meso = data.HasChild("money") ? (uint?)data["money"].ValueOrDefault<int>(0) : null;

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

        public static ItemReward Parse(WZObject data)
        {
            ItemReward result = new ItemReward();

            result.Id = data.HasChild("id") ? data["id"].ValueOrDefault<int>(-1) : -1;
            result.Count = data.HasChild("count") ? data["count"].ValueOrDefault<int>(0) : 0;
            result.PotentialGrade = data.HasChild("potentialGrade") ? data["potentialGrade"].ValueOrDefault<string>(null) : null;
            result.Gender = data.HasChild("gender") ? (bool?)(data["gender"].ValueOrDefault<int>(0) == 1) : null; // gender
            result.Job = data.HasChild("job") ? (int?)data["job"].ValueOrDefault<int>(0) : null;

            return result;
        }
    }

    public class SkillReward
    {
        public int Id; // id
        public IEnumerable<int> RequiredJobs; // job
        public int MasterLevel; // masterLevel
        public int SkillLevel; // skillLevel

        public static SkillReward Parse(WZObject data)
        {
            SkillReward result = new SkillReward();

            result.Id = data.HasChild("id") ? data["id"].ValueOrDefault<int>(-1) : -1;
            result.RequiredJobs = data.HasChild("job") ? data["job"].Select(c => c.ValueOrDefault<int>(-1)) : null; // job
            result.MasterLevel = data.HasChild("masterLevel") ? data["masterLevel"].ValueOrDefault<int>(-1) : -1;
            result.SkillLevel = data.HasChild("skillLevel") ? data["skillLevel"].ValueOrDefault<int>(-1) : -1;

            return result;
        }
    }
}
