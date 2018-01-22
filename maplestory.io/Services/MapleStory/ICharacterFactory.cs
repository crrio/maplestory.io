using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using WZData.MapleStory.Characters;
using WZData.MapleStory.Images;

namespace maplestory.io.Services.MapleStory
{
    public interface ICharacterFactory : INeedWZ<ICharacterFactory>
    {
        int[] GetSkinIds();
        CharacterSkin GetSkin(int id);
        Image<Rgba32> GetBase(int id, string animation = null, int frame = 0, bool showEars = false, bool showLefEars = false, int padding = 2, RenderMode renderMode = RenderMode.Full);
        Image<Rgba32> GetBaseWithHair(int id, string animation = null, int frame = 0, bool showEars = false, bool showLefEars = false, int padding = 2, int faceId = 20305, int hairId = 37831, RenderMode renderMode = RenderMode.Full);
        Image<Rgba32> GetCharacter(int id, string animation = null, int frame = 0, bool showEars = false, bool showLefEars = false, int padding = 2, string name = null, float zoom = 1, bool flipX = false, RenderMode renderMode = RenderMode.Full, params Tuple<int, string, float?>[] items);
        string[] GetActions(params int[] itemEntries);
        byte[] GetSpriteSheet(HttpRequest request, int id, bool showEars = false, bool showLefEars = false, int padding = 2, RenderMode renderMode = RenderMode.Full, SpriteSheetFormat format = SpriteSheetFormat.Plain, params Tuple<int, float?>[] itemEntries);
        IEnumerable<Tuple<Frame, Point, float?>> GetJsonCharacter(int id, string animation = null, int frame = 0, bool showEars = false, bool showLefEars = false, int padding = 2, string name = null, float zoom = 1, bool flipX = false, params Tuple<int, string, float?>[] items);
        Tuple<Image<Rgba32>, Dictionary<string, Point>, Dictionary<string, int>, int> GetDetailedCharacter(int id, string animation = null, int frame = 0, bool showEars = false, bool showLefEars = false, int padding = 2, string name = null, float zoom = 1, bool flipX = false, RenderMode renderMode = RenderMode.Full, params Tuple<int, string, float?>[] items);
    }
}
