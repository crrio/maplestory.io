using System;
using WZData.MapleStory;

namespace maplestory.io.Services.MapleStory
{
    public class ZMapFactory : IZMapFactory
    {
        private readonly ZMap zmap;

        public ZMapFactory(IWZFactory factory)
        {
            this.zmap = ZMap.Parse(factory.GetWZFile(WZ.Base).MainDirectory);
        }

        public ZMap GetZMap() => this.zmap;
    }
}
