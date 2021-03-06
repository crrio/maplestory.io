﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;
using maplestory.io.Data.Quests;

namespace maplestory.io.Services.Interfaces.MapleStory
{
    public interface IQuestFactory
    {
        IEnumerable<QuestMeta> GetQuests(string searchFor = null, int startPosition = 0, int? count = null);
        Quest GetQuest(int id);
    }

    public class QuestMeta
    {
        public int Id;
        public string Name;
        public byte? MinLevel;
        public DateTime? AvailabilityStart, AvailabilityEnd;
        public IEnumerable<int> QuestsAvailableAfterComplete;

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
