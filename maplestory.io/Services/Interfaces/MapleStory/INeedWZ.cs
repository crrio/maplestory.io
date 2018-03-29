using PKG1;

namespace maplestory.io.Services.Interfaces.MapleStory
{
    public interface INeedWZ<K> {
        K GetWithWZ(Region region, string version);
    }
}