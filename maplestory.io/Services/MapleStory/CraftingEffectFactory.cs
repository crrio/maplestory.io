using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;
using WZData;

namespace maplestory.io.Services.MapleStory
{
    public class CraftingEffectFactory : NeedWZ<ICraftingEffectFactory>, ICraftingEffectFactory
    {
        private readonly Dictionary<CraftingType, FrameBook> effects;
        private string[] EffectNames;

        public CraftingEffectFactory(IWZFactory wzFactory) : base(wzFactory)
            => EffectNames = Enum.GetNames(typeof(CraftingType));

        public CraftingEffectFactory(IWZFactory wzFactory, Region region, string version) : base(wzFactory, region, version)
            => EffectNames = Enum.GetNames(typeof(CraftingType));

        public string[] EffectList() => effects.Keys.Select(c => c.ToString()).ToArray();
        public FrameBook GetEffect(CraftingType crafting) {
            return FrameBook.ParseSingle(wz.Resolve($"Effect/CharacterEff/MeisterEff/{crafting.ToString()}"));
        }
        public FrameBook GetEffect(string crafting) => GetEffect((CraftingType)Enum.Parse(typeof(CraftingType), crafting, true));

        public override ICraftingEffectFactory GetWithWZ(Region region, string version)
            => new CraftingEffectFactory(_factory, region, version);
    }
}
