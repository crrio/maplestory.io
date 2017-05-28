using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WZData.ItemMetaInfo
{
    public class ChairInfo
    {
        public int recoveryHP;
        public int recoveryMP;
        public int reqLevel;

        public static ChairInfo Parse(WZObject info)
        {
            if (!(info.HasChild("recoveryHP") || info.HasChild("recoveryMP"))) return null;

            ChairInfo results = new ChairInfo();

            if (info.HasChild("recoveryHP")) results.recoveryHP = info["recoveryHP"].ValueOrDefault<int>(0);
            if (info.HasChild("recoveryMP")) results.recoveryMP = info["recoveryMP"].ValueOrDefault<int>(0);
            if (info.HasChild("reqLevel")) results.reqLevel = info["reqLevel"].ValueOrDefault<int>(0);

            return results;
        }
    }
}
