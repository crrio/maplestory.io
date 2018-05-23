using PKG1;
using maplestory.io.Data;

namespace maplestory.io.Services.Interfaces.MapleStory
{
    public interface IZMapFactory
    {
        ZMap GetZMap();
        SMap GetSMap();
    }
}
