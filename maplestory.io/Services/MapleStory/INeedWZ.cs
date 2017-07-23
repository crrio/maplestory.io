using PKG1;

namespace maplestory.io.Services.MapleStory
{
    public interface INeedWZ<K> {
        K GetWithWZ(Region region, string version);
    }
}