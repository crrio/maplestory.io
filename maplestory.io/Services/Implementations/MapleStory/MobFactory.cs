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
        public IEnumerable<Frame> GetFrames(int mobId, string frameBook) => GetMob(mobId)?.GetFrameBook(frameBook)?.First().frames;
        public IEnumerable<MobInfo> GetMobs(int startPosition = 0, int? count = null, int? minLevelFilter = null, int? maxLevelFilter = null, string searchFor = null)
            => WZ.Resolve("String/Mob").Children
                .Select(MobInfo.Parse)
                .Where(c => (!minLevelFilter.HasValue || c.Level >= minLevelFilter) && (!maxLevelFilter.HasValue || c.Level <= maxLevelFilter) && (string.IsNullOrEmpty(searchFor) || c.Name.Contains(searchFor)))
                .Skip(startPosition)
                .Take(count ?? int.MaxValue); // MaxValue isn't a nice alternative, but it should probably work
    }
}
