using Microsoft.AspNetCore.Mvc;
using PKG1;
using maplestory.io.Data.Maps;

namespace maplestory.io.Controllers.API
{
    [Produces("application/json")]
    [Route("api/{region}/{version}/map")]
    public class MapController : APIController
    {
        [Route("")]
        [HttpGet]
        [ProducesResponseType(typeof(MapName[]), 200)]
        public IActionResult List()
            => Json(MapFactory.GetMapNames());

        [Route("{mapId}")]
        [HttpGet]
        [ProducesResponseType(typeof(Map), 200)]
        public IActionResult GetMap(int mapId)
        {
            return Json(MapFactory.GetMap(mapId));
        }

        [Route("{mapId}/name")]
        [HttpGet]
        [ProducesResponseType(typeof(MapName), 200)]
        public IActionResult GetMapName(int mapId)
        {
            return Json(MapFactory.GetMapName(mapId));
        }

        [Route("icon/{markName}")]
        [HttpGet]
        [Produces("image/png")]
        public IActionResult GetMarkByName(string markName)
        {
            return File(MapFactory.GetMapMark(markName).Mark.ImageToByte(Request), "image/png");
        }

        [Route("{mapId}/icon")]
        [HttpGet]
        [Produces("image/png")]
        public IActionResult GetMapMark(int mapId)
        {
            Map map = MapFactory.GetMap(mapId);
            return GetMarkByName(map.MapMark);
        }

        [Route("{mapId}/render")]
        [HttpGet]
        [Produces("image/png")]
        public IActionResult RenderMap(int mapId, [FromQuery]bool showLife = false, [FromQuery]bool showPortals = false, [FromQuery]bool showBackgrounds = false)
            => File(MapFactory.Render(mapId, showLife, showPortals, showBackgrounds).ImageToByte(Request), "image/png");

        [Route("{mapId}/minimap")]
        [HttpGet]
        [Produces("image/png")]
        public IActionResult GetMinimap(int mapId)
            => File(MapFactory.GetMap(mapId).MiniMap.canvas.ImageToByte(Request), "image/png");

        [Route("{mapId}/bgm")]
        [HttpGet]
        [Produces("audio/mpeg")]
        public IActionResult GetBGM(int mapId)
        {
            Map map = MapFactory.GetMap(mapId);
            return File(MusicFactory.GetSong(map.BackgroundMusic), "audio/mpeg");
        }
    }
}