using System;
using WZData.MapleStory;

namespace maplestory.io.Services.MapleStory
{
    public class ZMapFactory : IZMapFactory
    {
        private readonly ZMap zmap;
        private readonly SMap smap;

        public ZMapFactory(IWZFactory factory)
        {
            this.zmap = ZMap.Parse(factory.GetWZFile(WZ.Base).MainDirectory);
            this.smap = SMap.Parse(factory.GetWZFile(WZ.Base).MainDirectory);
        }

        public SMap GetSMap() => this.smap;
        public ZMap GetZMap() => this.zmap;
    }
}
