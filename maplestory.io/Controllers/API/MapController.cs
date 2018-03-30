using maplestory.io.Data.Maps;
using Microsoft.AspNetCore.Mvc;

namespace maplestory.io.Controllers.API
{
    [Route("api/{region}/{version}/map")]
    public class MapController : APIController
    {
        [Route("")]
        [HttpGet]
        public IActionResult List()
            => Json(MapFactory.GetMapNames());

        [Route("{mapId}")]
        [HttpGet]
        public IActionResult GetMap(int mapId)
        {
            return Json(MapFactory.GetMap(mapId));
        }

        [Route("{mapId}/name")]
        [HttpGet]
        public IActionResult GetMapName(int mapId)
        {
            return Json(MapFactory.GetMapName(mapId));
        }

        [Route("icon/{markName}")]
        [HttpGet]
        public IActionResult GetMarkByName(string markName)
        {
            return File(MapFactory.GetMapMark(markName).Mark.ImageToByte(Request), "image/png");
        }

        [Route("{mapId}/icon")]
        [HttpGet]
        public IActionResult GetMapMark(int mapId)
        {
            Map map = MapFactory.GetMap(mapId);
            return GetMarkByName(map.MapMark);
        }

        [Route("{mapId}/render")]
        [HttpGet]
        public IActionResult RenderMap(int mapId, [FromQuery]bool showLife = false, [FromQuery]bool showPortals = false, [FromQuery]bool showBackgrounds = false)
            => File(MapFactory.Render(mapId, showLife, showPortals, showBackgrounds).ImageToByte(Request), "image/png");

        [Route("{mapId}/minimap")]
        [HttpGet]
        public IActionResult GetMinimap(int mapId)
            => File(MapFactory.GetMap(mapId).MiniMap.canvas.ImageToByte(Request), "image/png");

        [Route("{mapId}/bgm")]
        [HttpGet]
        public IActionResult GetBGM(int mapId)
        {
            Map map = MapFactory.GetMap(mapId);
            return File(MusicFactory.GetSong(map.BackgroundMusic), "audio/mpeg");
        }
    }
}