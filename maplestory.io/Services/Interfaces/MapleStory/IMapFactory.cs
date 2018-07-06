using maplestory.io.Data.Maps;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;

namespace maplestory.io.Services.Interfaces.MapleStory
{
    public interface IMapFactory
    {
        IEnumerable<MapName> GetMapNames(string searchFor = null, int startPosition = 0, int? count = null);
        MapName GetMapName(int id);
        Map GetMap(int id, bool followLinks = true);
        MapMark GetMapMark(string markName);
        Image<Rgba32> Render(int id, int frame, bool showLife, bool showPortals, bool showBackgrounds);
        Image<Rgba32> RenderLayer(int mapId, int layer, int frame);
        Image<Rgba32> RenderLayer(int mapId, int layer, int frame, bool filterTrash, int? minX, int? minY);
        MapRenderPlan GetRenderPlan(int mapId);
    }
}
