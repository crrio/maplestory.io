using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;

namespace WZData
{
    public class ItemPotentialLevel
    {
        public int PotentialId, Level;
        public List<Tuple<string, string>> Modifiers;

        public static IEnumerable<ItemPotentialLevel> Parse(int potentialId, WZProperty levels)
        {
            if (levels == null)
                yield break;

            foreach(WZProperty level in levels.Children.Values)
            {
                int potLevel = 0;

                if (!int.TryParse(level.Name, out potLevel))
                    continue;

                yield return new ItemPotentialLevel()
                {
                    Level = potLevel,
                    Modifiers = level
                        .Children
                        .Select(c => new Tuple<string, string>(c.Key, Convert.ToString(((IWZPropertyVal) c.Value).GetValue())))
                        .ToList(),
                    PotentialId = potentialId
                };
            }
        }
    }
}
