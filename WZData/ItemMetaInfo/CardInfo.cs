using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;

namespace WZData.ItemMetaInfo
{
    public class CardInfo
    {
        public int mob;

        public static CardInfo Parse(WZProperty info)
        {
            if (info.Children.ContainsKey("mob"))
                return new CardInfo() { mob = info.ResolveFor<int>("mob") ?? 0 };
            else return null;
        }
    }
}
