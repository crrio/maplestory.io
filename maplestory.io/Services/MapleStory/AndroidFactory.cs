using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WZData.MapleStory;

namespace maplestory.io.Services.MapleStory
{
    public class AndroidFactory : IAndroidFactory
    {
        private readonly Dictionary<int, Func<Android>> androidLookup;
        private readonly int[] androidIds;

        public AndroidFactory(IWZFactory wzFactory)
        {
            this.androidLookup = Android.GetLookup(wzFactory.GetWZFile(WZ.Etc)).ToDictionary(c => c.Item1, c => c.Item2);
            this.androidIds = androidLookup.Keys.ToArray();
        }

        public Android GetAndroid(int androidId) => androidLookup[androidId]();
        public int[] GetAndroidIDs() => androidIds;
    }
}
