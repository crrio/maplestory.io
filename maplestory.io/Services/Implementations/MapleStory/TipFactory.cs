using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;
using maplestory.io.Data;
using maplestory.io.Services.Interfaces.MapleStory;

namespace maplestory.io.Services.Implementations.MapleStory
{
    public class TipFactory : NeedWZ, ITipFactory
    {
        public IEnumerable<Tips> GetTips() => Tips.GetTips(WZ.Resolve("Etc"));
    }
}
