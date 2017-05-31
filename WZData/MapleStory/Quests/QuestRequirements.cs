using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WZData.MapleStory.Quests
{
    public class QuestRequirements
    {
        public int Id;
        public IEnumerable<int> Jobs; // job
        public IEnumerable<int> RequiredFieldsEntered; // fieldEnter
        public DateTime? StartTime; // start
        public DateTime? EndTime; // end
        public byte? LevelMinimum; // lvmin
        public byte? LevelMaximum; // lvmax
        public IEnumerable<Requirement> Mobs; // mob
        public IEnumerable<Requirement> Items; // item
        public IEnumerable<Requirement> Quests; // quest
        public int? NPCId; // npc
        public DayOfWeek? OnDayOfWeek; // dayOfWeek
        public IEnumerable<Requirement> Pet; // pet
        public int? PetTamenessMin; // pettamenessmin
        public bool? DayByDay; // dayByDay
        public bool? NormalAutoStart; // normalAutoStart
        public int? MinimumMonsterBookCards; // mbmin
        public QuestState State;

        public static QuestRequirements[] Parse(WZObject data)
        {
            int id = int.Parse(data.Name);
            QuestRequirements onStart = data.HasChild("0") && data["0"].ChildCount > 0 ? QuestRequirements.Parse(id, data["0"], QuestState.Start) : null;
            QuestRequirements onComplete = data.HasChild("1") && data["1"].ChildCount > 0 ? QuestRequirements.Parse(id, data["1"], QuestState.Complete) : null;

            return new QuestRequirements[] { onStart, onComplete };
        }

        public static QuestRequirements Parse(int id, WZObject data, QuestState state)
        {
            QuestRequirements result = new QuestRequirements();

            result.Id = id;
            result.State = state;
            result.Jobs = data.HasChild("job") ? data["job"].Select(c => c.ValueOrDefault<int>(0)) : null; // job
            result.RequiredFieldsEntered = data.HasChild("fieldEnter") ? data["fieldEnter"].Select(c => c.ValueOrDefault<int>(0)) : null; // fieldEnter
            result.StartTime = data.HasChild("start") ? (DateTime?)ResolveDateTimeString(data["start"].ValueOrDefault<string>("")) : null;
            result.EndTime = data.HasChild("end") ? (DateTime?)ResolveDateTimeString(data["end"].ValueOrDefault<string>("")) : null;
            result.LevelMinimum = data.HasChild("lvmin") ? (byte?) data["lvmin"].ValueOrDefault<int>(0) : null;
            result.LevelMaximum = data.HasChild("lvmax") ? (byte?) data["lvmax"].ValueOrDefault<int>(0) : null;
            result.Mobs = data.HasChild("mob") ? data["mob"].Select(c => Requirement.Parse(c)) : null;
            result.Items = data.HasChild("item") ? data["item"].Select(c => Requirement.Parse(c)) : null;
            result.Quests = data.HasChild("quest") ? data["quest"].Select(c => Requirement.Parse(c)) : null;
            result.NPCId = data.HasChild("npc") ? (int?) data["npc"].ValueOrDefault<int>(0) : null;
            result.OnDayOfWeek = data.HasChild("dayOfWeek") ? (DayOfWeek?)ResolveDayOfWeek(data["dayOfWeek"].ValueOrDefault("")) : null; // dayOfWeek
            result.Pet = data.HasChild("pet") ? data["pet"].Select(c => Requirement.Parse(c)) : null;
            result.PetTamenessMin = data.HasChild("pettamenessmin") ? (int?) data["pettamenessmin"].ValueOrDefault<int>(0) : null;
            result.DayByDay = data.HasChild("dayByDay") && data["dayByDay"].ValueOrDefault<int>(0) == 1;
            result.NormalAutoStart = data.HasChild("normalAutoStart") && data["normalAutoStart"].ValueOrDefault<int>(0) == 1;
            result.MinimumMonsterBookCards = data.HasChild("mbmin") ? (int?) data["mbmin"].ValueOrDefault<int>(0) : null;

            return result;
        }

        static DayOfWeek ResolveDayOfWeek(string v)
        {
            Dictionary<string, string> days = Enum.GetNames(typeof(DayOfWeek)).ToDictionary(c => c.Substring(0, 3), c => c);
            return (DayOfWeek)Enum.Parse(typeof(DayOfWeek), days.ContainsKey(v.ToLower()) ? days[v.ToLower()] : "Sunday");
        }

        static DateTime ResolveDateTimeString(string dt)
        {
            switch(dt.Length)
            {
                case 12:
                    return new DateTime(int.Parse(dt.Substring(0, 4)), int.Parse(dt.Substring(4, 2)), int.Parse(dt.Substring(6, 2)), int.Parse(dt.Substring(8, 2)), int.Parse(dt.Substring(10, 2)), 0);
                case 8:
                    return new DateTime(int.Parse(dt.Substring(0, 4)), int.Parse(dt.Substring(4, 2)), int.Parse(dt.Substring(6, 2)));
            }

            return DateTime.MinValue;
        }
    }

    /// <summary>
    /// This is left vague because it can be a Mob Requirement, Item Requirement, or it can be a Quest Requirement
    /// </summary>
    public class Requirement
    {
        public int? Count; // count
        public int? Id; // id
        public int? State; // state

        public static Requirement Parse(WZObject c)
        {
            Requirement result = new Requirement();

            result.Count = c.HasChild("count") ? (int?)c["count"].ValueOrDefault<int>(0) : null;
            result.Id = c.HasChild("id") ? (int?)c["id"].ValueOrDefault<int>(0) : null;
            result.State = c.HasChild("state") ? (int?)c["state"].ValueOrDefault<int>(0) : null;

            return result;
        }
    }
}
