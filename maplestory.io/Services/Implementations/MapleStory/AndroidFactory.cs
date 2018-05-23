using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;
using maplestory.io.Data;
using maplestory.io.Services.Interfaces.MapleStory;

namespace maplestory.io.Services.Implementations.MapleStory
{
    public class AndroidFactory : NeedWZ, IAndroidFactory
    {
        public Android GetAndroid(int androidId) {
            return Android.Parse(WZ.Resolve($"Etc/Android/{androidId.ToString("D4")}"), androidId);
        }
        public IEnumerable<int> GetAndroidIDs() {
            return WZ.Resolve("Etc/Android").Children.Select(c => int.Parse(c.NameWithoutExtension));
        }
    }
}
