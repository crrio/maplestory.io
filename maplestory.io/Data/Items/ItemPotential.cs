using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;

namespace maplestory.io.Data
{
    public class ItemPotential
    {
        public int id, OptionType, RequiredLevel;
        public string Message;

        public static Tuple<ItemPotential, IEnumerable<ItemPotentialLevel>> Parse(WZProperty potentialEntry)
        {
            ItemPotential potential = new ItemPotential();
            if (!int.TryParse(potentialEntry.NameWithoutExtension, out potential.id))
                return null;

            WZProperty info = potentialEntry.Resolve("info");

            if (info.Children.Any(c => c.NameWithoutExtension.Equals("string")))
                potential.Message = info.ResolveForOrNull<string>("string");
            else return null;

            potential.OptionType = info.ResolveFor<int>("optionType") ?? 0;
            potential.RequiredLevel = info.ResolveFor<int>("reqLevel") ?? 0;

            return new Tuple<ItemPotential, IEnumerable<ItemPotentialLevel>>(potential, ItemPotentialLevel.Parse(potential.id, potentialEntry.Resolve("level")));
        }
    }
}
