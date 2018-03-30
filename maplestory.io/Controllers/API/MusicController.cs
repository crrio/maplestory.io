using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace maplestory.io.Controllers.API
{
    [Route("api/{region}/{version}/music")]
    public class MusicController : APIController
    {
        [Route("")]
        [HttpGet]
        public IActionResult List() => Json(MusicFactory.GetSounds());

        [Route("{*songPath}")]
        [HttpGet]
        public IActionResult Song(string songPath)
        {
            if (MusicFactory.DoesSoundExist(songPath)) return File(MusicFactory.GetSong(songPath), "audio/mpeg");

            string[] paths = MusicFactory.GetSounds().Where(c => c.StartsWith(songPath)).ToArray();
            if (paths.Length > 0) return Json(paths);

            return NotFound();
        }
    }
}