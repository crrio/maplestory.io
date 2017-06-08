using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WZData.MapleStory;

namespace maplestory.io.Services.MapleStory
{
    public interface IAndroidFactory
    {
        int[] GetAndroidIDs();
        Android GetAndroid(int androidId);
    }
}
