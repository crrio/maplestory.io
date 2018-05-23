using System;
using PKG1;
using System.Collections.Concurrent;
using maplestory.io.Services.Interfaces.MapleStory;
using maplestory.io.Models;

namespace maplestory.io.Services.Implementations.MapleStory
{
    public abstract class NeedWZ {
        public Region Region { get; set; }
        public string Version { get; set; }
        public MSPackageCollection WZ { get; set; }

        public void CloneWZFrom(NeedWZ original)
        {
            WZ = original.WZ;
            Region = original.Region;
            Version = original.Version;
        }
    }
}