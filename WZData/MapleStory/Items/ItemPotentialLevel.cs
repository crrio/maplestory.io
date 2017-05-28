using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using reWZ.WZProperties;

namespace WZData
{
    public class ItemPotentialLevel
    {
        public int PotentialId, Level;
        public List<Tuple<string, string>> Modifiers;

        public static IEnumerable<ItemPotentialLevel> Parse(int potentialId, WZObject levels)
        {
            if (levels == null)
                yield break;

            foreach(WZObject level in levels)
            {
                int potLevel = 0;

                if (!int.TryParse(level.Name, out potLevel))
                    continue;

                yield return new ItemPotentialLevel()
                {
                    Level = potLevel,
                    Modifiers = level
                        .ToList()
                        .Select(c => new Tuple<string, string>(c.Name, c.ValueOrDefault<string>(null) ?? c.ValueOrDefault<int>(0).ToString()))
                        .ToList(),
                    PotentialId = potentialId
                };
            }
        }
    }
}
