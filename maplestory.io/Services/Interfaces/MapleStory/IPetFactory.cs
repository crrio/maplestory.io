using maplestory.io.Data.Items;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;

namespace maplestory.io.Services.Interfaces.MapleStory
{
    public interface IPetFactory
    {
        IEnumerable<KeyValuePair<int, string>> GetPets(int startPosition = 0, int? count = null);
        Pet GetPet(int petId);
        Image<Rgba32> RenderPet(int petId, string animation, int frame, int petEquip);
    }
}
