using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using maplestory.io.Services.MapleStory;
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
        public IActionResult List()
            => Json(_factory.GetMapNames());

        [Route("{mapId}")]
        public IActionResult GetMap(int mapId)
        {
            return Json(_factory.GetMap(mapId));
        }

        [Route("{mapId}/name")]
        public IActionResult GetMapName(int mapId)
        {
            return Json(_factory.GetMapName(mapId));
        }

        [Route("mark/{markName}")]
        public IActionResult GetMarkByName(string markName)
        {
            return File((byte[])new ImageConverter().ConvertTo(_factory.GetMapMark(markName).Mark, typeof(byte[])), "image/png");
        }

        [Route("{mapId}/mark")]
        public IActionResult GetMapMark(int mapId)
        {
            Map map = _factory.GetMap(mapId);
            return GetMarkByName(map.MapMark);
        }

        [Route("{mapId}/bgm")]
        public IActionResult GetBGM(int mapId)
        {
            Map map = _factory.GetMap(mapId);
            return File(_musicFactory.GetSong(map.BackgroundMusic), "audio/mpeg");
        }
    }
}