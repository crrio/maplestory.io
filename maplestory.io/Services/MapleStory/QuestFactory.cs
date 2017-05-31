using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WZData.MapleStory.Quests;

namespace maplestory.io.Services.MapleStory
{
    public class QuestFactory : IQuestFactory
    {
        Dictionary<int, Quest> allQuests;
        List<QuestMeta> questMeta;

        public QuestFactory(IWZFactory factory)
        {
            IEnumerable<Quest> quests = Quest.GetQuests(factory.GetWZFile(WZ.Quest).MainDirectory);

            allQuests = new Dictionary<int, Quest>();
            questMeta = new List<QuestMeta>();
            foreach (Quest q in quests)
            {
                allQuests.Add(q.Id, q);
                questMeta.Add(new QuestMeta(q.Id, q.Name, q.RequirementToStart?.LevelMinimum, q.RequirementToStart?.StartTime, q.RequirementToStart?.EndTime));
            }
        }

        public Quest GetQuest(int id) => allQuests[id];

        public IEnumerable<QuestMeta> GetQuests() => questMeta.ToArray();
    }
}
