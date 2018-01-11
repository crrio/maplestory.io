using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WZData.MapleStory.Items;
using SixLabors.ImageSharp;

namespace maplestory.io.Services.MapleStory
{
    public interface IPetFactory : INeedWZ<IPetFactory>
    {
        Dictionary<int, string> GetPets();
        Pet GetPet(int petId);
        Image<Rgba32> RenderPet(int petId, string animation, int frame, int petEquip);
    }
}
