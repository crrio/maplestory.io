using System;
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
        public IEnumerable<QuestMeta> GetQuests() {
            IEnumerable<Quest> quests = Quest.GetQuests(WZ.Resolve("Quest"));
            return quests.Select(q => new QuestMeta(
                q.Id,
                q.Name,
                q.RequirementToStart?.LevelMinimum,
                q.RequirementToStart?.StartTime,
                q.RequirementToStart?.EndTime
            ));
        }
    }
}
