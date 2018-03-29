using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;
using maplestory.io.Data;
using maplestory.io.Data.Images;
using maplestory.io.Services.Interfaces.MapleStory;

namespace maplestory.io.Services.Implementations.MapleStory
{
    public class CraftingEffectFactory : NeedWZ, ICraftingEffectFactory
    {
        private static string[] EffectNames;

        static CraftingEffectFactory()
            => EffectNames = Enum.GetNames(typeof(CraftingType));

        public string[] EffectList() => EffectNames;
        public FrameBook GetEffect(CraftingType crafting) {
            return FrameBook.ParseSingle(WZ.Resolve($"Effect/CharacterEff/MeisterEff/{crafting.ToString()}"));
        }
        public FrameBook GetEffect(string crafting) => GetEffect((CraftingType)Enum.Parse(typeof(CraftingType), crafting, true));
    }
}
