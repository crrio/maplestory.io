using maplestory.io.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace maplestory.io.Controllers
{
    [Produces("application/json")]
    [Route("api/character")]
    public class CharacterController : Controller
    {
        readonly string ContentRootPath;
        public CharacterController(IHostingEnvironment env)
        {
            ContentRootPath = env.ContentRootPath;
        }

        [Route("{characterName}")]
        public async Task<IActionResult> GetCharacter(string characterName)
        {
            var character = await Character.GetCharacter(characterName);
            return Json(character);
        }

        [Route("{characterName}/avatar")]
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