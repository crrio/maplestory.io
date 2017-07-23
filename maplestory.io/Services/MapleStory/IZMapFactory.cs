using PKG1;
using WZData.MapleStory;

namespace maplestory.io.Services.MapleStory
{
    public interface IZMapFactory : INeedWZ<IZMapFactory>
    {
        ZMap GetZMap();
        SMap GetSMap();
    }
}
