using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;
using WZData.MapleStory.Quests;

namespace maplestory.io.Services.MapleStory
{
    public class QuestFactory : NeedWZ<IQuestFactory>, IQuestFactory
    {
        public QuestFactory(IWZFactory factory) : base(factory) { }
        public QuestFactory(IWZFactory factory, Region region, string version) : base(factory, region, version) { }

        public Quest GetQuest(int id)
            => Quest.GetQuest(wz.Resolve("Quest"), id);
        public IEnumerable<QuestMeta> GetQuests() {
            IEnumerable<Quest> quests = Quest.GetQuests(wz.Resolve("Quest"));
            return quests.Select(q => new QuestMeta(
                q.Id,
                q.Name,
                q.RequirementToStart?.LevelMinimum,
                q.RequirementToStart?.StartTime,
                q.RequirementToStart?.EndTime
            ));
        }
        public override IQuestFactory GetWithWZ(Region region, string version)
            => new QuestFactory(_factory, region, version);
    }
}
