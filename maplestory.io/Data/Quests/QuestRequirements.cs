using PKG1;
using System;
using System.Collections.Generic;
using System.Linq;

namespace maplestory.io.Data.Quests
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
        public NPC.NPC NPCInfo;
        public DayOfWeek[] OnDayOfWeek; // dayOfWeek
        public bool? AnyPet;
        public IEnumerable<Requirement> Pet; // pet
        public int? PetTamenessMin; // pettamenessmin
        public bool? DayByDay; // dayByDay
        public bool? NormalAutoStart; // normalAutoStart
        public int? MinimumMonsterBookCards; // mbmin
        public QuestState State;

        public static QuestRequirements[] Parse(WZProperty data)
        {
            if (data == null) return null;
            int id = int.Parse(data.NameWithoutExtension);
            QuestRequirements onStart = QuestRequirements.Parse(id, data.Resolve("0"), QuestState.Start);
            QuestRequirements onComplete = QuestRequirements.Parse(id, data.Resolve("1"), QuestState.Complete);

            return new QuestRequirements[] { onStart, onComplete };
        }

        public static QuestRequirements Parse(int id, WZProperty data, QuestState state)
        {
            QuestRequirements result = new QuestRequirements();

            result.Id = id;
            result.State = state;
            result.Jobs = data.Resolve("job")?.Children.Select(c => Convert.ToInt32(((IWZPropertyVal)c).GetValue())); // job
            result.RequiredFieldsEntered = data.Resolve("fieldEnter")?.Children.Select(c => Convert.ToInt32(((IWZPropertyVal)c).GetValue())); // fieldEnter
            result.StartTime = (DateTime?)ResolveDateTimeString(data.ResolveForOrNull<string>("start"));
            result.EndTime = (DateTime?)ResolveDateTimeString(data.ResolveForOrNull<string>("end"));
            result.LevelMinimum = data.ResolveFor<byte>("lvmin");
            result.LevelMaximum = data.ResolveFor<byte>("lvmax");
            result.Mobs = data.Resolve("mob")?.Children.Select(c => Requirement.Parse(c));
            result.Items = data.Resolve("item")?.Children.Select(c => Requirement.Parse(c));
            result.Quests = data.Resolve("quest")?.Children.Select(c => Requirement.Parse(c));
            result.NPCId = data.ResolveFor<int>("npc");
            string dayOfWeek = data.ResolveForOrNull<string>("dayOfWeek");
            result.OnDayOfWeek = ResolveDayOfWeek(dayOfWeek != null ? new string[] { dayOfWeek } : data.Resolve("dayOfWeek")?.Children?.Select(c => c.NameWithoutExtension).ToArray()); // dayOfWeek
            result.AnyPet = data.ResolveFor<bool>("allPet");
            result.Pet = data.Resolve("pet")?.Children.Select(c => Requirement.Parse(c));
            result.PetTamenessMin = data.ResolveFor<int>("pettamenessmin");
            result.DayByDay = data.ResolveFor<bool>("dayByDay");
            result.NormalAutoStart = data.ResolveFor<bool>("normalAutoStart");
            result.MinimumMonsterBookCards = data.ResolveFor<int>("mbmin");

            return result;
        }

        static DayOfWeek[] ResolveDayOfWeek(params string[] daysOfWeek)
        {
            if (daysOfWeek == null) return null;
            Dictionary<string, string> days = Enum.GetNames(typeof(DayOfWeek)).ToDictionary(c => c.Substring(0, 3).ToLower(), c => c);
            return daysOfWeek.Select(v =>
            {
                return (DayOfWeek)Enum.Parse(typeof(DayOfWeek), days.ContainsKey(v.ToLower()) ? days[v.ToLower()] : "Sunday");
            }).ToArray();
        }

        static DateTime? ResolveDateTimeString(string dt)
        {
            if (dt == null) return null;

            switch (dt.Length)
            {
                case 12:
                    return new DateTime(int.Parse(dt.Substring(0, 4)), int.Parse(dt.Substring(4, 2)), int.Parse(dt.Substring(6, 2)), int.Parse(dt.Substring(8, 2)) % 24, int.Parse(dt.Substring(10, 2)) % 60, 0);
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
