using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;

namespace maplestory.io.Data.Items
{
    public class CashInfo
    {
        public bool cash;

        public static CashInfo Parse(WZProperty info)
        {
            bool? isCash = info.ResolveFor<bool>("cash");
            if (isCash.HasValue)
                return new CashInfo() { cash = isCash ?? false };
            else
                return null;
        }
    }
}
