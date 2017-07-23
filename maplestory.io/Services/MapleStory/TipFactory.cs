using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;
using WZData.MapleStory;

namespace maplestory.io.Services.MapleStory
{
    public class TipFactory : NeedWZ<ITipFactory>, ITipFactory
    {
        public TipFactory(IWZFactory factory) : base(factory) { }

        public TipFactory(IWZFactory factory, Region region, string version) : base(factory, region, version) { }

        public IEnumerable<Tips> GetTips() => Tips.GetTips(_factory.GetWZ(region, version).Resolve("Etc"));

        public override ITipFactory GetWithWZ(Region region, string version)
            => new TipFactory(_factory, region, version);
    }
}
