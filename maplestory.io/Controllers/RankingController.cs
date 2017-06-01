using maplestory.io.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace maplestory.io.Controllers
{
    [Produces("application/json")]
    [Route("api/ranking")]
    public class RankingController : Controller
    {
        readonly string ContentRootPath;
        public RankingController(IHostingEnvironment env)
        {
            ContentRootPath = env.ContentRootPath;
        }

        [Route("{characterName}")]
        [HttpGet]
        [ProducesResponseType(typeof(Character), 200)]
        public async Task<IActionResult> GetCharacter(string characterName)
        {
            var character = await Character.GetCharacter(characterName);
            return Json(character);
        }

        [Route("{characterName}/avatar")]
        [HttpGet]
        [ProducesResponseType(200)]
        [Produces("image/png")]
        public async Task<IActionResult> GetCharacterAvatar(string characterName)
        {
            var character = await Character.GetCharacter(characterName);

            byte[] avatarData = System.IO.File.ReadAllBytes(Path.Combine(ContentRootPath, "wwwroot", "images/no-avatar.png"));
            try
            {
                avatarData = await character.GetAvatar();
            } catch (Exception ex)
            {
                // Nexon's Avatar stuff is broken atm, we probably don't need to log this.
            }

            return File(avatarData, "image/png");
        }
    }
}