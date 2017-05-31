using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WZData.MapleStory.Quests
{
    public class Quest
    {
        public int Id;
        public IEnumerable<string> Messages;
        public bool? AutoPreComplete; // autoPreComplete;
        public bool? AutoStart; // autoStart
        public bool? AutoCancel; // autoCancel
        public bool? AutoComplete; // autoComplete
        public bool? AutoAccept; // autoAccept
        public string Name; // name
        public int? Area; // area
        public string DemandSummary; // demandSummary
        public string RewardSummary; // rewardSummary
        public string PlaceSummary;
        public string Summary; // summary
        public bool? DisableAtCompleteTab; // disableAtCompleteTab
        public bool? DisableAtNPC; // disableAtNPC
        public bool? DisableAtPerformTab; // disableAtPerformTab
        public bool? DisableAtStartTab; // disableAtStartTab
        public bool? Blocked; // blocked
        public bool? ResignBlocked;
        public string Type; // type
        public int? MedalCategory; // medalCategory
        public int? MedalId; // viewMedalItem
        public bool? StartNavigation; // startNavi
        public int? TargetMapId; // targetMapId
        public int? TimeLimit; // timeLimit
        public string TimerUI; // timerUI
        public bool? HasSelectedMob; // selectedMob
        public bool? StraightStart; // straightStart
        public IEnumerable<int> ValidMaps; // validField
        public bool? ShowEffect; // showEffect
        public IEnumerable<int> DeleteItems; // deleteItem

        public QuestRewards RewardOnStart;
        public QuestRewards RewardOnComplete;
        public QuestRequirements RequirementToStart;
        public QuestRequirements RequirementToComplete;

        public static Quest Parse(WZObject data)
        {
            Quest result = new Quest();

            result.Id = int.Parse(data.Name);
            result.AutoPreComplete = data.HasChild("autoPreComplete") ? (bool?)(data["autoPreComplete"].ValueOrDefault<int>(0) == 1) : null;
            result.AutoStart = data.HasChild("autoStart") ? (bool?)(data["autoStart"].ValueOrDefault<int>(0) == 1) : null;
            result.AutoCancel = data.HasChild("autoCancel") ? (bool?)(data["autoCancel"].ValueOrDefault<int>(0) == 1) : null;
            result.AutoComplete = data.HasChild("autoComplete") ? (bool?)(data["autoComplete"].ValueOrDefault<int>(0) == 1) : null;
            result.AutoAccept = data.HasChild("autoAccept") ? (bool?)(data["autoAccept"].ValueOrDefault<int>(0) == 1) : null;
            result.Name = data.HasChild("name") ? data["name"].ValueOrDefault<string>(null) : null;
            result.Area = data.HasChild("area") ? (int?)data["area"].ValueOrDefault<int>(0) : null; // 
            result.DemandSummary = data.HasChild("demandSummary") ? data["demandSummary"].ValueOrDefault<string>(null) : null;
            result.RewardSummary = data.HasChild("rewardSummary") ? data["rewardSummary"].ValueOrDefault<string>(null) : null;
            result.PlaceSummary = data.HasChild("placeSummary") ? data["placeSummary"].ValueOrDefault<string>(null) : null;
            result.Summary = data.HasChild("summary") ? data["summary"].ValueOrDefault<string>(null) : null;
            result.DisableAtCompleteTab = data.HasChild("disableAtCompleteTab") ? (bool?)(data["disableAtCompleteTab"].ValueOrDefault<int>(0) == 1) : null;
            result.DisableAtNPC = data.HasChild("disableAtNPC") ? (bool?)(data["disableAtNPC"].ValueOrDefault<int>(0) == 1) : null;
            result.DisableAtPerformTab = data.HasChild("disableAtPerformTab") ? (bool?)(data["disableAtPerformTab"].ValueOrDefault<int>(0) == 1) : null;
            result.DisableAtStartTab = data.HasChild("disableAtStartTab") ? (bool?)(data["disableAtStartTab"].ValueOrDefault<int>(0) == 1) : null;
            result.Blocked = data.HasChild("blocked") ? (bool?)(data["blocked"].ValueOrDefault<int>(0) == 1) : null;
            result.ResignBlocked = data.HasChild("resignBlocked") ? (bool?)(data["resignBlocked"].ValueOrDefault<int>(0) == 1) : null;
            result.Type = data.HasChild("type") ? data["type"].ValueOrDefault<string>(null) : null;
            result.MedalCategory = data.HasChild("medalCategory") ? (int?)data["medalCategory"].ValueOrDefault<int>(0) : null; // 
            result.MedalId = data.HasChild("viewMedalItem") ? (int?)data["viewMedalItem"].ValueOrDefault<int>(0) : null; // 
            result.StartNavigation = data.HasChild("startNavi") ? (bool?)(data["startNavi"].ValueOrDefault<int>(0) == 1) : null;
            result.TargetMapId = data.HasChild("targetMapId") ? (int?)data["targetMapId"].ValueOrDefault<int>(0) : null; // 
            result.TimeLimit = data.HasChild("timeLimit") ? (int?)data["timeLimit"].ValueOrDefault<int>(0) : null; // 
            result.TimerUI = data.HasChild("timerUI") ? data["timerUI"].ValueOrDefault<string>(null) : null;
            result.HasSelectedMob = data.HasChild("selectedMob") ? (bool?)(data["selectedMob"].ValueOrDefault<int>(0) == 1) : null;
            result.StraightStart = data.HasChild("straightStart") ? (bool?)(data["straightStart"].ValueOrDefault<int>(0) == 1) : null;
            result.ValidMaps = data.HasChild("validField") ? data["validField"].Select(c => c.ValueOrDefault<int>(-1)) : null;
            result.ShowEffect = data.HasChild("showEffect") ? (bool?)(data["showEffect"].ValueOrDefault<int>(0) == 1) : null;
            result.Messages = data.Where(c => int.TryParse(c.Name, out int bogus)).Select(c => c.ValueOrDefault<string>(null)).Where(c => c != null);
            result.DeleteItems = data.HasChild("deleteItem") ? data["deleteItem"].Select(c => c.ValueOrDefault<int>(-1)) : null;

            return result;
        }

        public static IEnumerable<Quest> GetQuests(WZObject questWz)
        {
            Dictionary<int, QuestRewards[]> rewards = questWz["Act.img"]
                .AsParallel()
                .Select(QuestRewards.Parse)
                .Select(c => c.Where(b => b != null).ToArray())
                .Where(c => c.Length > 0)
                .ToDictionary(c => c.First().Id, c => c);
            Dictionary<int, QuestRequirements[]> requirements = questWz["Check.img"]
                .AsParallel()
                .Select(QuestRequirements.Parse)
                .Select(c => c.Where(b => b != null).ToArray())
                .Where(c => c.Length > 0)
                .ToDictionary(c => c.First().Id, c => c);

            return questWz["QuestInfo.img"]
                .AsParallel()
                .Select(Quest.Parse)
                .Select(c =>
                {
                    QuestRewards[] questRewards = rewards.ContainsKey(c.Id) ? rewards[c.Id] : null;
                    QuestRequirements[] questRequirements = requirements.ContainsKey(c.Id) ? requirements[c.Id] : null;

                    c.RequirementToComplete = questRequirements?.Where(b => b.State == QuestState.Complete).FirstOrDefault();
                    c.RequirementToStart = questRequirements?.Where(b => b.State == QuestState.Start).FirstOrDefault();
                    c.RewardOnStart = questRewards?.Where(b => b.State == QuestState.Start).FirstOrDefault();
                    c.RewardOnComplete= questRewards?.Where(b => b.State == QuestState.Complete).FirstOrDefault();

                    return c;
                });
        }
    }

    public enum QuestState
    {
        Start = 0,
        Complete = 1
    }
}
