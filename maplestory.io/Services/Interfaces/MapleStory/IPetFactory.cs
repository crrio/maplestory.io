﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using maplestory.io.Data.Items;
using SixLabors.ImageSharp;

namespace maplestory.io.Services.Interfaces.MapleStory
{
    public interface IPetFactory
    {
        Dictionary<int, string> GetPets();
        Pet GetPet(int petId);
        Image<Rgba32> RenderPet(int petId, string animation, int frame, int petEquip);
    }
}