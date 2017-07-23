using System;
using PKG1;
using System.Collections.Concurrent;

namespace maplestory.io.Services.MapleStory
{
    public abstract class NeedWZ<K> : INeedWZ<K> {
        public NeedWZ(IWZFactory _factory, Region region, string version) : this(_factory) {
            this.region = region;
            this.version = version;
            this.wz = _factory.GetWZ(region, version);
        }
        public NeedWZ(IWZFactory _factory) { this._factory = _factory; }
        public Region region;
        public string version;
        public PackageCollection wz;
        public IWZFactory _factory;
        public abstract K GetWithWZ(Region region, string version);
    }
}