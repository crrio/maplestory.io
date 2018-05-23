using System;
using PKG1;
using maplestory.io.Data;
using maplestory.io.Services.Interfaces.MapleStory;

namespace maplestory.io.Services.Implementations.MapleStory
{
    public class ZMapFactory : NeedWZ, IZMapFactory
    {
        public SMap GetSMap()
            => SMap.Parse(WZ.Resolve("Base"));
        public ZMap GetZMap()
            => ZMap.Parse(WZ.Resolve("Base"));
    }
}
