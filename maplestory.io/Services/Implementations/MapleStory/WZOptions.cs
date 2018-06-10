using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;

namespace maplestory.io.Services.Implementations.MapleStory
{
    public class WZOptions
    {
        public WZVersion[] versions { get; set; }
    }

    public class WZVersion {
        public string path { get; set; }
        public Region region { get; set; }
        public string version { get; set; }
    }
}
