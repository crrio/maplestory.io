using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;

namespace maplestory.io.Data.Items
{
    public class SlotInfo
    {
        public int slotMax;

        public static SlotInfo Parse(WZProperty info)
        {
            SlotInfo result = new SlotInfo();
            if (info.Children.Any(c => c.NameWithoutExtension.Equals("slotMax")))
                result.slotMax = info.ResolveFor<int>("slotMax") ?? 0;
            else
                return null;

            return result;
        }
    }
}
