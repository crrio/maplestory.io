using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;

namespace WZData.ItemMetaInfo
{
    public class SlotInfo
    {
        public int slotMax;

        public static SlotInfo Parse(WZProperty info)
        {
            SlotInfo result = new SlotInfo();
            if (info.Children.ContainsKey("slotMax"))
                result.slotMax = info.ResolveFor<int>("slotMax") ?? 0;
            else
                return null;

            return result;
        }
    }
}
