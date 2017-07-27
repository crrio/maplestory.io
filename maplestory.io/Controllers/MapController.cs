using maplestory.io.Services.MapleStory;
using Microsoft.AspNetCore.Mvc;
using ImageSharp;
using WZData.MapleStory.Maps;
using PKG1;

namespace maplestory.io.Controllers
{
    [Produces("application/json")]
    [Route("api/{region}/{version}/map")]
    public class MapController : Controller
    {
        [FromRoute]
        public Region region { get; set; }
        [FromRoute]
        public string version { get; set; }

        private IMapFactory _factory;
        private IMusicFactory _musicFactory;

        public MapController(IMapFactory factory, IMusicFactory musicFactory)
        {
            _factory = factory;
            _musicFactory = musicFactory;
        }
        [Route("")]
        [HttpGet]
        [ProducesResponseType(typeof(MapName[]), 200)]
        public IActionResult List()
            => Json(_factory.GetWithWZ(region, version).GetMapNames());

        [Route("{mapId}")]
        [HttpGet]
        [ProducesResponseType(typeof(Map), 200)]
        public IActionResult GetMap(int mapId)
        {
            return Json(_factory.GetWithWZ(region, version).GetMap(mapId));
        }

        [Route("{mapId}/name")]
        [HttpGet]
        [ProducesResponseType(typeof(MapName), 200)]
        public IActionResult GetMapName(int mapId)
        {
            return Json(_factory.GetWithWZ(region, version).GetMapName(mapId));
        }

        [Route("icon/{markName}")]
        [HttpGet]
        [Produces("image/png")]
        public IActionResult GetMarkByName(string markName)
        {
            return File(_factory.GetWithWZ(region, version).GetMapMark(markName).Mark.ImageToByte(), "image/png");
        }

        [Route("{mapId}/icon")]
        [HttpGet]
        [Produces("image/png")]
        public IActionResult GetMapMark(int mapId)
        {
            Map map = _factory.GetWithWZ(region, version).GetMap(mapId);
            return GetMarkByName(map.MapMark);
        }

        [Route("{mapId}/render")]
        [HttpGet]
        [Produces("image/png")]
        public IActionResult RenderMap(int mapId)
            => File(_factory.GetWithWZ(region, version).Render(mapId).ImageToByte(), "image/png");

        [Route("{mapId}/minimap")]
        [HttpGet]
        [Produces("image/png")]
        public IActionResult GetMinimap(int mapId)
            => File(_factory.GetWithWZ(region, version).GetMap(mapId).MiniMap.canvas.ImageToByte(), "image/png");

        [Route("{mapId}/bgm")]
        [HttpGet]
        [Produces("audio/mpeg")]
        public IActionResult GetBGM(int mapId)
        {
            Map map = _factory.GetWithWZ(region, version).GetMap(mapId);
            return File(_musicFactory.GetSong(map.BackgroundMusic), "audio/mpeg");
        }
    }
}