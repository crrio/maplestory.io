using System;
using System.Collections.Generic;
using ImageSharp;
using System.Linq;
using System.Threading.Tasks;
using WZData.MapleStory.Characters;

namespace maplestory.io.Services.MapleStory
{
    public interface ICharacterFactory
    {
        int[] GetSkinIds();
        CharacterSkin GetSkin(int id);
        Image<Rgba32> GetBase(int id, string animation = null, int frame = 0, bool showEars = false, int padding = 2);
        Image<Rgba32> GetBaseWithHair(int id, string animation = null, int frame = 0, bool showEars = false, int padding = 2, int faceId = 20305, int hairId = 37831);
        Image<Rgba32> GetCharacter(int id, string animation = null, int frame = 0, bool showEars = false, int padding = 2, params Tuple<int, string>[] items);
        Image<Rgba32> GetCharacter(int id, string animation = null, int frame = 0, bool showEars = false, int padding = 2, string renderMode = "default", params Tuple<int, string>[] items);
        string[] GetActions(params int[] itemEntries);
    }
}
