using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;

namespace maplestory.io.Data.Items
{
    public class CardInfo
    {
        public int mob;

        public static CardInfo Parse(WZProperty info)
        {
            if (info.Children.Any(c => c.NameWithoutExtension.Equals("mob")))
                return new CardInfo() { mob = info.ResolveFor<int>("mob") ?? 0 };
            else return null;
        }
    }
}
