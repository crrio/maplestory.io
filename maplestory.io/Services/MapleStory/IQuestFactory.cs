using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WZData.MapleStory.Quests;

namespace maplestory.io.Services.MapleStory
{
    public interface IQuestFactory
    {
        IEnumerable<QuestMeta> GetQuests();
        Quest GetQuest(int id);
    }

    public class QuestMeta
    {
        public int Id;
        public string Name;
        public byte? MinLevel;
        public DateTime? AvailabilityStart, AvailabilityEnd;

        public QuestMeta(int id, string name, byte? minLevel, DateTime? availabilityStart, DateTime? availabilityEnd)
        {
            Id = id;
            Name = name;
            MinLevel = minLevel;
            AvailabilityStart = availabilityStart;
            AvailabilityEnd = availabilityEnd;
        }
    }
}
