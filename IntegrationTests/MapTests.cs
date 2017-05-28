using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.Linq;
using WZData.MapleStory.Maps;
using maplestory.io.Services.MapleStory;

namespace IntegrationTests
{
    class MapTests
    {
        [Fact]
        public void NamesLoad()
        {
            MapName[] names = MapName.GetMapNames(new WZFactory(null).GetWZFile(WZ.String)).ToArray();
        }
    }
}
