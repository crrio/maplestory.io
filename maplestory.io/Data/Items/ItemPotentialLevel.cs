using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;

namespace maplestory.io.Data
{
    public class ItemPotentialLevel
    {
        public int PotentialId, Level;
        public List<Tuple<string, string>> Modifiers;

        public static IEnumerable<ItemPotentialLevel> Parse(int potentialId, WZProperty levels)
        {
            if (levels == null)
                yield break;

            foreach(WZProperty level in levels.Children)
            {
                int potLevel = 0;

                if (!int.TryParse(level.NameWithoutExtension, out potLevel))
                    continue;

                yield return new ItemPotentialLevel()
                {
                    Level = potLevel,
                    Modifiers = level
                        .Children
                        .Select(c => new Tuple<string, string>(c.NameWithoutExtension, Convert.ToString(((IWZPropertyVal) c).GetValue())))
                        .ToList(),
                    PotentialId = potentialId
                };
            }
        }
    }
}
