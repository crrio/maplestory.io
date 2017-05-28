using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WZData.ItemMetaInfo
{
    public class SlotInfo
    {
        public int slotMax;

        public static SlotInfo Parse(WZObject info)
        {
            SlotInfo result = new SlotInfo();
            if (info.HasChild("slotMax"))
                result.slotMax = info["slotMax"].ValueOrDefault<int>(1);
            else
                return null;

            return result;
        }
    }
}
