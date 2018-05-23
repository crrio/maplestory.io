using maplestory.io.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace maplestory.io.Controllers.API
{
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
        public async Task<IActionResult> GetCharacter(string characterName)
        {
            var character = await Character.GetCharacter(characterName);
            return Json(character);
        }

        [Route("{characterName}/avatar")]
        [HttpGet]
        public async Task<IActionResult> GetCharacterAvatar(string characterName)
        {
            var character = await Character.GetCharacter(characterName);
            byte[] avatarData = System.IO.File.ReadAllBytes(Path.Combine(ContentRootPath, "wwwroot", "images/no-avatar.png"));

            try
            {
                avatarData = await character.GetAvatar();
            }
            catch (Exception ex)
            {
                // Nexon's Avatar stuff is broken atm, we probably don't need to log this.
            }

            return File(avatarData, "image/png");
        }

        [Route("{characterName}/avatar/full")]
        [HttpGet]
        public async Task<IActionResult> GetFullCharacterAvatar(string characterName)
        {
            var character = await Character.GetCharacter(characterName);
            CharacterLook look = character.GetAvatarLook();

            int[] items = new int[] { look.Cap, look.Cape, look.Coat, look.EarAccessory, look.EyeAccessory, look.Face, look.FaceAccessory, look.Glove, look.Hair, look.Pants, look.Shield, look.Shoes, look.Weapon }
                .Where(c => c != -1).ToArray();

            return LocalRedirect($"/api/gms/latest/character/{2000 + look.Skin}/{string.Join(",", items)}");
        }
    }
}