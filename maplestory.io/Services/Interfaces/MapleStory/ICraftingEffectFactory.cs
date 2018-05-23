using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;
using maplestory.io.Data;
using maplestory.io.Data.Images;

namespace maplestory.io.Services.Interfaces.MapleStory
{
    public interface ICraftingEffectFactory
    {
        maplestory.io.Data.Images.FrameBook GetEffect(CraftingType crafting);
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
