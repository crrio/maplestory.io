using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WZData.MapleStory;

namespace maplestory.io.Services.MapleStory
{
    public class TipFactory : ITipFactory
    {
        IEnumerable<Tips> data;
        public TipFactory(IWZFactory factory)
        {
            data = Tips.GetTips(factory.GetWZFile(WZ.Etc).MainDirectory);
        }

        public IEnumerable<Tips> GetTips() => data;
    }
}
