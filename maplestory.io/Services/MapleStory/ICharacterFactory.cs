using System;
using System.Collections.Generic;
using ImageSharp;
using System.Linq;
using System.Threading.Tasks;
using WZData.MapleStory.Characters;
using PKG1;
using Microsoft.AspNetCore.Http;

namespace maplestory.io.Services.MapleStory
{
    public interface ICharacterFactory : INeedWZ<ICharacterFactory>
    {
        int[] GetSkinIds();
        CharacterSkin GetSkin(int id);
        Image<Rgba32> GetBase(int id, string animation = null, int frame = 0, bool showEars = false, int padding = 2, RenderMode renderMode = RenderMode.Full);
        Image<Rgba32> GetBaseWithHair(int id, string animation = null, int frame = 0, bool showEars = false, int padding = 2, int faceId = 20305, int hairId = 37831, RenderMode renderMode = RenderMode.Full);
        Image<Rgba32> GetCharacter(int id, string animation = null, int frame = 0, bool showEars = false, int padding = 2, RenderMode renderMode = RenderMode.Full, params Tuple<int, string>[] items);
        string[] GetActions(params int[] itemEntries);
        byte[] GetSpriteSheet(HttpRequest request, int id, bool showEars = false, int padding = 2, RenderMode renderMode = RenderMode.Full, params int[] itemEntries);
    }
}
