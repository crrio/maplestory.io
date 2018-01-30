using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;
using maplestory.io.Services.Rethink;

namespace maplestory.io.Services.MapleStory
{
    public class WZRethinkStore : IWZFactory
    {
        public WZRethinkStore(IRethinkDbConnectionFactory factory)
        {

        }

        public PackageCollection GetWZ(Region region, string version)
        {
            throw new NotImplementedException();
        }
    }
}
