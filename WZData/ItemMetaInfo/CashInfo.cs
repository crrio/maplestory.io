using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WZData.ItemMetaInfo
{
    public class CashInfo
    {
        public bool cash;

        public static CashInfo Parse(WZObject info)
        {
            if (info.HasChild("cash"))
                return new CashInfo()
                {
                    cash = (byte)info["cash"].ValueOrDefault<int>(0) == 1
                };
            else
                return null;
        }
    }
}
