using maplestory.io.Services.MapleStory;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using WZData.MapleStory.Maps;

namespace maplestory.io.Controllers
{
    [Produces("application/json")]
    [Route("api/map")]
    public class MapController : Controller
    {
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
            => Json(_factory.GetMapNames());

        [Route("{mapId}")]
        [HttpGet]
        [ProducesResponseType(typeof(Map), 200)]
        public IActionResult GetMap(int mapId)
        {
            return Json(_factory.GetMap(mapId));
        }

        [Route("{mapId}/name")]
        [HttpGet]
        [ProducesResponseType(typeof(MapName), 200)]
        public IActionResult GetMapName(int mapId)
        {
            return Json(_factory.GetMapName(mapId));
        }

        [Route("icon/{markName}")]
        [HttpGet]
        [Produces("image/png")]
        public IActionResult GetMarkByName(string markName)
        {
            return File((byte[])new ImageConverter().ConvertTo(_factory.GetMapMark(markName).Mark, typeof(byte[])), "image/png");
        }

        [Route("{mapId}/icon")]
        [HttpGet]
        [Produces("image/png")]
        public IActionResult GetMapMark(int mapId)
        {
            Map map = _factory.GetMap(mapId);
            return GetMarkByName(map.MapMark);
        }

        [Route("{mapId}/bgm")]
        [HttpGet]
        [Produces("audio/mpeg")]
        public IActionResult GetBGM(int mapId)
        {
            Map map = _factory.GetMap(mapId);
            return File(_musicFactory.GetSong(map.BackgroundMusic), "audio/mpeg");
        }
    }
}