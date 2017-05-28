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
        public IActionResult Song(string songPath) => File(_factory.GetSong(songPath), "application/ogg");
    }
}