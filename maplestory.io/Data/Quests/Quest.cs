﻿using maplestory.io.Models;
using PKG1;
using System.Collections.Generic;
using System.Linq;

namespace maplestory.io.Data.Quests
{
    public class QuestName
    {
        public int id;
        public string name;
    }

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
        public string AreaName;
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
        public IEnumerable<QuestRequirements> QuestsAvailableOnComplete;

        public QuestRewards RewardOnStart;
        public QuestRewards RewardOnComplete;
        public QuestRequirements RequirementToStart;
        public QuestRequirements RequirementToComplete;

        public static Quest Parse(WZProperty data)
        {
            if (data == null) return null;
            Quest result = new Quest();

            result.Id = int.Parse(data.NameWithoutExtension);
            result.AutoPreComplete = data.ResolveFor<bool>("autoPreComplete");
            result.AutoStart = data.ResolveFor<bool>("autoStart");
            result.AutoCancel = data.ResolveFor<bool>("autoCancel");
            result.AutoComplete = data.ResolveFor<bool>("autoComplete");
            result.AutoAccept = data.ResolveFor<bool>("autoAccept");
            result.Name = data.ResolveForOrNull<string>("name");
            result.Area = data.ResolveFor<int>("area");
            if (result.Area.HasValue && data.FileContainer.Collection is MSPackageCollection)
                ((MSPackageCollection)(data.FileContainer.Collection)).QuestAreaNames.TryGetValue(result.Area.Value, out result.AreaName);
            result.DemandSummary = data.ResolveForOrNull<string>("demandSummary");
            result.RewardSummary = data.ResolveForOrNull<string>("rewardSummary");
            result.PlaceSummary = data.ResolveForOrNull<string>("placeSummary");
            result.Summary = data.ResolveForOrNull<string>("summary");
            result.DisableAtCompleteTab = data.ResolveFor<bool>("disableAtCompleteTab");
            result.DisableAtNPC = data.ResolveFor<bool>("disableAtNPC");
            result.DisableAtPerformTab = data.ResolveFor<bool>("disableAtPerformTab");
            result.DisableAtStartTab = data.ResolveFor<bool>("disableAtStartTab");
            result.Blocked = data.ResolveFor<bool>("blocked");
            result.ResignBlocked = data.ResolveFor<bool>("resignBlocked");
            result.Type = data.ResolveForOrNull<string>("type");
            result.MedalCategory = data.ResolveFor<int>("medalCategory");
            result.MedalId = data.ResolveFor<int>("viewMedalItem");
            result.StartNavigation = data.ResolveFor<bool>("startNavi");
            result.TargetMapId = data.ResolveFor<int>("targetMapId");
            result.TimeLimit = data.ResolveFor<int>("timeLimit");
            result.TimerUI = data.ResolveForOrNull<string>("timerUI");
            result.HasSelectedMob = data.ResolveFor<bool>("selectedMob");
            result.StraightStart = data.ResolveFor<bool>("straightStart");
            result.ValidMaps = data.Resolve("validField")?.Children.Select(c => ((WZPropertyVal<int>)c).Value);
            result.ShowEffect = data.ResolveFor<bool>("showEffect");
            result.Messages = data.Children.Where(c => int.TryParse(c.NameWithoutExtension, out int bogus)).Select(c => ((IWZPropertyVal)c).GetValue().ToString()).Where(c => !string.IsNullOrEmpty(c));
            result.DeleteItems = data.Resolve("deleteItem")?.Children.Select(c => ((WZPropertyVal<int>)c).Value);

            return result;
        }

        public static Quest GetQuest(WZProperty questWz, int questId) {
            QuestRewards[] rewards = QuestRewards.Parse(questWz.Resolve($"Act/{questId}")) ?? new QuestRewards[0];
            QuestRequirements[] requirements = QuestRequirements.Parse(questWz.Resolve($"Check/{questId}")) ?? new QuestRequirements[0];
            Quest quest = Quest.Parse(questWz.Resolve($"QuestInfo/{questId}"));
            if (quest == null) return null;

            quest.RequirementToComplete = requirements?.Where(b => b != null && b.State == QuestState.Complete).FirstOrDefault();
            quest.RequirementToStart = requirements?.Where(b => b != null && b.State == QuestState.Start).FirstOrDefault();
            quest.RewardOnStart = rewards?.Where(b => b != null && b.State == QuestState.Start).FirstOrDefault();
            quest.RewardOnComplete= rewards?.Where(b => b != null && b.State == QuestState.Complete).FirstOrDefault();
            if (questWz.FileContainer.Collection is MSPackageCollection) {
                MSPackageCollection collection = (MSPackageCollection) questWz.FileContainer.Collection;
                if (collection.AvailableOnCompleteTable.ContainsKey(quest.Id))
                    quest.QuestsAvailableOnComplete = collection.AvailableOnCompleteTable[quest.Id];
            }

            return quest;
        }

        public static IEnumerable<Quest> GetQuests(WZProperty questWz)
        {
            Dictionary<int, QuestRewards[]> rewards = questWz.Resolve("Act").Children
                .AsParallel()
                .Select(QuestRewards.Parse)
                .Select(c => c.Where(b => b != null).ToArray())
                .Where(c => c.Length > 0)
                .ToDictionary(c => c.First().Id, c => c);
            Dictionary<int, QuestRequirements[]> requirements = questWz.Resolve("Check").Children
                .AsParallel()
                .Select(QuestRequirements.Parse)
                .Select(c => c.Where(b => b != null).ToArray())
                .Where(c => c.Length > 0)
                .ToDictionary(c => c.First().Id, c => c);

            return questWz.Resolve("QuestInfo").Children
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
