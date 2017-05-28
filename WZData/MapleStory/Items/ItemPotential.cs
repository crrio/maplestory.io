using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using reWZ.WZProperties;

namespace WZData
{
    public class ItemPotential
    {
        public int id, OptionType, RequiredLevel;
        public string Message;

        public static Tuple<ItemPotential, IEnumerable<ItemPotentialLevel>> Parse(WZObject pot)
        {
            ItemPotential potential = new ItemPotential();
            if (!int.TryParse(pot.Name, out potential.id))
                return null;

            WZObject info = pot.ResolvePath("info");

            if (info.HasChild("string"))
                potential.Message = info["string"].ValueOrDie<string>();
            else return null;

            if (info.HasChild("optionType"))
                potential.OptionType = info["optionType"].ValueOrDefault<int>(0);

            if (info.HasChild("reqLevel"))
                potential.RequiredLevel = info["reqLevel"].ValueOrDefault<int>(0);

            return new Tuple<ItemPotential, IEnumerable<ItemPotentialLevel>>(potential, ItemPotentialLevel.Parse(potential.id, pot.ResolvePath("level")));
        }
    }
}
