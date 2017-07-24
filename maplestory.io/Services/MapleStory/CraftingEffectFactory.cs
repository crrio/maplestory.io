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
        private static string[] EffectNames;

        static CraftingEffectFactory()
            => EffectNames = Enum.GetNames(typeof(CraftingType));

        public CraftingEffectFactory(IWZFactory wzFactory) : base(wzFactory) { }
        public CraftingEffectFactory(IWZFactory wzFactory, Region region, string version) : base(wzFactory, region, version) { }

        public string[] EffectList() => EffectNames;
        public FrameBook GetEffect(CraftingType crafting) {
            return FrameBook.ParseSingle(wz.Resolve($"Effect/CharacterEff/MeisterEff/{crafting.ToString()}"));
        }
        public FrameBook GetEffect(string crafting) => GetEffect((CraftingType)Enum.Parse(typeof(CraftingType), crafting, true));

        public override ICraftingEffectFactory GetWithWZ(Region region, string version)
            => new CraftingEffectFactory(_factory, region, version);
    }
}
