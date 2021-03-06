﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;
using maplestory.io.Data.Quests;
using maplestory.io.Services.Interfaces.MapleStory;

namespace maplestory.io.Services.Implementations.MapleStory
{
    public class QuestFactory : NeedWZ, IQuestFactory
    {
        public Quest GetQuest(int id)
            => Quest.GetQuest(WZ.Resolve("Quest"), id);
        public IEnumerable<QuestMeta> GetQuests(string searchFor = null, int startPosition = 0, int? count = null) {
            if (!string.IsNullOrEmpty(searchFor)) searchFor.ToLower();
            IEnumerable<Quest> quests = Quest.GetQuests(WZ.Resolve("Quest")).OrderBy(c => c.Id);
            return quests.Select(q =>
                new QuestMeta(
                    q.Id,
                    q.Name,
                    q.RequirementToStart?.LevelMinimum,
                    q.RequirementToStart?.StartTime,
                    q.RequirementToStart?.EndTime
                )
            )
            .Where(c => string.IsNullOrEmpty(searchFor) || (!string.IsNullOrEmpty(c.Name) && c.Name.ToLower().Contains(searchFor)))
            .Skip(startPosition)
            .Take(count ?? int.MaxValue);
        }
    }
}
