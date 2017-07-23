using System;
using PKG1;
using WZData.MapleStory;

namespace maplestory.io.Services.MapleStory
{
    public class ZMapFactory : NeedWZ<IZMapFactory>, IZMapFactory
    {
        public ZMapFactory(IWZFactory factory) : base(factory) { }
        public ZMapFactory(IWZFactory factory, Region region, string version) : base(factory, region, version) { }

        public SMap GetSMap()
            => SMap.Parse(wz.Resolve("Base"));
        public ZMap GetZMap()
            => ZMap.Parse(wz.Resolve("Base"));
        public override IZMapFactory GetWithWZ(Region region, string version)
            => new ZMapFactory(_factory, region, version);
    }
}
