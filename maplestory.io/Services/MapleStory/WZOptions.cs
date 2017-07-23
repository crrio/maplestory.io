using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;

namespace maplestory.io.Services.MapleStory
{
    public class WZOptions
    {
        public WZVersion[] versions;
    }

    public class WZVersion {
        public string path;
        public Region region;
        public string version;
    }
}
