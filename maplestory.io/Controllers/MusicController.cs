using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using maplestory.io.Services.MapleStory;
using reWZ;
using reWZ.WZProperties;

namespace maplestory.io.Controllers
{
    [Produces("application/json")]
    [Route("api/music")]
    public class MusicController : Controller
    {
        private IMusicFactory _factory;

        public MusicController(IMusicFactory factory)
        {
            _factory = factory;
        }

        [Route("")]
        public IActionResult List() => Json(_factory.GetSounds());

        [Route("{*songPath}")]
        public IActionResult Song(string songPath)
        {
            if (_factory.DoesSoundExist(songPath)) return File(_factory.GetSong(songPath), "audio/mpeg");

            string[] paths = _factory.GetSounds().Where(c => c.StartsWith(songPath)).ToArray();
            if (paths.Length > 0) return Json(paths);

            return NotFound();
        }
    }
}