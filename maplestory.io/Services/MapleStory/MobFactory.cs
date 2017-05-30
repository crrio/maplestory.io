using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WZData.MapleStory.Mobs;

namespace maplestory.io.Services.MapleStory
{
    public class MobFactory : IMobFactory
    {
        private Dictionary<int, Func<Mob>> mobLookup;
        private MobInfo[] allMobMeta;

        public MobFactory(IWZFactory factory)
        {
            Tuple<int, MobInfo, Func<Mob>>[] mobs = Mob.GetLookup(factory.GetWZFile(WZ.Mob).MainDirectory, factory.GetWZFile(WZ.String).MainDirectory).ToArray();
            mobLookup = mobs.ToDictionary(k => k.Item1, v => v.Item3);
            allMobMeta = mobs.Select(c => c.Item2).ToArray();
        }
        public Mob GetMob(int id) => mobLookup[id]();

        public IEnumerable<MobInfo> GetMobs() => allMobMeta;
    }
}
