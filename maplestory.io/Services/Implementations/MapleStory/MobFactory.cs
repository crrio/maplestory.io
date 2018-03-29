using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;
using maplestory.io.Data;
using maplestory.io.Data.Mobs;
using maplestory.io.Data.Images;
using maplestory.io.Services.Interfaces.MapleStory;

namespace maplestory.io.Services.Implementations.MapleStory
{
    public class MobFactory : NeedWZ, IMobFactory
    {
        public Mob GetMob(int id)
            => Mob.Parse(WZ.Resolve($"String/Mob/{id}"));
        public IEnumerable<MobInfo> GetMobs()
            => WZ.Resolve("String/Mob").Children.Select(MobInfo.Parse);
        public IEnumerable<Frame> GetFrames(int mobId, string frameBook) => GetMob(mobId)?.GetFrameBook(frameBook)?.First().frames;
    }
}
