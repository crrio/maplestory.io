using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;
using WZData.MapleStory;

namespace maplestory.io.Services.MapleStory
{
    public interface ITipFactory : INeedWZ<ITipFactory>
    {
        IEnumerable<Tips> GetTips();
    }
}
