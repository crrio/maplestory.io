using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;
using WZData;
using WZData.MapleStory.Mobs;

namespace maplestory.io.Services.MapleStory
{
    public class MobFactory : NeedWZ<IMobFactory>, IMobFactory
    {
        public MobFactory(IWZFactory factory) : base(factory) { }
        public MobFactory(IWZFactory factory, Region region, string version) : base(factory, region, version) { }

        public Mob GetMob(int id)
            => Mob.Parse(wz.Resolve($"String/Mob/{id}"));
        public IEnumerable<MobInfo> GetMobs()
            => wz.Resolve("String/Mob").Children.Values.Select(MobInfo.Parse);
        public IEnumerable<Frame> GetFrames(int mobId, string frameBook) => GetMob(mobId)?.GetFrameBook(frameBook)?.First().frames;

        public override IMobFactory GetWithWZ(Region region, string version)
            => new MobFactory(_factory, region, version);
    }
}
