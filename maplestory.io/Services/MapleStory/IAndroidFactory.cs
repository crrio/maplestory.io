using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;
using WZData.MapleStory;

namespace maplestory.io.Services.MapleStory
{
    public interface IAndroidFactory : INeedWZ<IAndroidFactory>
    {
        IEnumerable<int> GetAndroidIDs();
        Android GetAndroid(int androidId);
    }
}
