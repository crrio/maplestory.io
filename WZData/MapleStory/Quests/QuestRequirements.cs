using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PKG1;

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

        public static QuestRequirements[] Parse(WZProperty data)
        {
            int id = int.Parse(data.Name);
            QuestRequirements onStart = data.Children.ContainsKey("0") && data.Resolve("0").Children.Count > 0 ? QuestRequirements.Parse(id, data.Resolve("0"), QuestState.Start) : null;
            QuestRequirements onComplete = data.Children.ContainsKey("1") && data.Resolve("1").Children.Count > 0 ? QuestRequirements.Parse(id, data.Resolve("1"), QuestState.Complete) : null;

            return new QuestRequirements[] { onStart, onComplete };
        }

        public static QuestRequirements Parse(int id, WZProperty data, QuestState state)
        {
            QuestRequirements result = new QuestRequirements();

            result.Id = id;
            result.State = state;
            result.Jobs = data.Resolve("job")?.Children.Select(c => Convert.ToInt32(((IWZPropertyVal)c.Value).GetValue())); // job
            result.RequiredFieldsEntered = data.Resolve("fieldEnter")?.Children.Select(c => Convert.ToInt32(((IWZPropertyVal)c.Value).GetValue())); // fieldEnter
            result.StartTime = data.Children.ContainsKey("start") ? (DateTime?)ResolveDateTimeString(data.ResolveForOrNull<string>("start")) : null;
            result.EndTime = data.Children.ContainsKey("end") ? (DateTime?)ResolveDateTimeString(data.ResolveForOrNull<string>("end")) : null;
            result.LevelMinimum = data.ResolveFor<byte>("lvmin");
            result.LevelMaximum = data.ResolveFor<byte>("lvmax");
            result.Mobs = data.Resolve("mob")?.Children.Values.Select(c => Requirement.Parse(c));
            result.Items = data.Resolve("item")?.Children.Values.Select(c => Requirement.Parse(c));
            result.Quests = data.Resolve("quest")?.Children.Values.Select(c => Requirement.Parse(c));
            result.NPCId = data.ResolveFor<int>("npc");
            result.OnDayOfWeek = data.Children.ContainsKey("dayOfWeek") ? (DayOfWeek?)ResolveDayOfWeek(data.ResolveForOrNull<string>("dayOfWeek")) : null; // dayOfWeek
            result.Pet = data.Resolve("pet")?.Children.Values.Select(c => Requirement.Parse(c));
            result.PetTamenessMin = data.ResolveFor<int>("pettamenessmin");
            result.DayByDay = data.ResolveFor<bool>("dayByDay");
            result.NormalAutoStart = data.ResolveFor<bool>("normalAutoStart");
            result.MinimumMonsterBookCards = data.ResolveFor<int>("mbmin");

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

        public static Requirement Parse(WZProperty c)
        {
            Requirement result = new Requirement();

            result.Count = c.ResolveFor<int>("count");
            result.Id = c.ResolveFor<int>("id");
            result.State = c.ResolveFor<int>("state");

            return result;
        }
    }
}
