using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WZData.ItemMetaInfo
{
    public class CardInfo
    {
        public int mob;

        public static CardInfo Parse(WZObject info)
        {
            if (info.HasChild("mob"))
                return new CardInfo()
                {
                    mob = info["mob"].ValueOrDefault<int>(0)
                };
            else return null;
        }
    }
}
