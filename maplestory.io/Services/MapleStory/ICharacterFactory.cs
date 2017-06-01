using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using WZData.MapleStory.Characters;

namespace maplestory.io.Services.MapleStory
{
    public interface ICharacterFactory
    {
        int[] GetSkinIds();
        CharacterSkin GetSkin(int id);
        Bitmap GetBase(int id, string animation = "stand1", int frame = 0, bool showEars = false, int padding = 2);
        Bitmap GetBaseWithHair(int id, string animation = "stand1", int frame = 0, bool showEars = false, int padding = 2, int faceId = 20305, int hairId = 37831);
        Bitmap GetCharacter(int id, string animation = "stand1", int frame = 0, bool showEars = false, int padding = 2, params int[] items);
    }
}
