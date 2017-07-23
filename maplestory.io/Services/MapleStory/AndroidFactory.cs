using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;
using WZData.MapleStory;

namespace maplestory.io.Services.MapleStory
{
    public class AndroidFactory : NeedWZ<IAndroidFactory>, IAndroidFactory
    {
        public AndroidFactory(IWZFactory wzFactory, Region region, string version) : base(wzFactory, region, version) { }
        public Android GetAndroid(int androidId) {
            return Android.Parse(wz.Resolve($"Etc/Android/{androidId.ToString("D4")}"), androidId);
        }
        public IEnumerable<int> GetAndroidIDs() {
            return wz.Resolve("Etc/Android").Children.Keys.Select(c => int.Parse(c));
        }

        public override IAndroidFactory GetWithWZ(Region region, string version)
            => new AndroidFactory(_factory, region, version);
    }
}
