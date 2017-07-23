﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;
using WZData;

namespace maplestory.io.Services.MapleStory
{
    public interface ICraftingEffectFactory : INeedWZ<ICraftingEffectFactory>
    {
        FrameBook GetEffect(CraftingType crafting);
        FrameBook GetEffect(string crafting);
        string[] EffectList();
    }

    public enum CraftingType
    {
        Accessory,
        Alchemy,
        Equipment
    }
}
