using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using maplestory.io.Services.MapleStory;
using PKG1;

namespace maplestory.io.Controllers
{
    [Produces("application/json")]
    [Route("api/{region}/{version}/music")]
    public class MusicController : Controller
    {
        [FromRoute]
        public Region region { get; set; }
        [FromRoute]
        public string version { get; set; }

        private IMusicFactory _factory;

        public MusicController(IMusicFactory factory)
        {
            _factory = factory;
        }

        [Route("")]
        [HttpGet]
        [ProducesResponseType(typeof(string[]), 200)]
        public IActionResult List() => Json(_factory.GetWithWZ(region, version).GetSounds());

        [Route("{*songPath}")]
        [HttpGet]
        [Produces("audio/mpeg")]
        public IActionResult Song(string songPath)
        {
            if (_factory.GetWithWZ(region, version).DoesSoundExist(songPath)) return File(_factory.GetWithWZ(region, version).GetSong(songPath), "audio/mpeg");

            string[] paths = _factory.GetWithWZ(region, version).GetSounds().Where(c => c.StartsWith(songPath)).ToArray();
            if (paths.Length > 0) return Json(paths);

            return NotFound();
        }
    }
}