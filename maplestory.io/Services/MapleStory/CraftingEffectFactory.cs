using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WZData;

namespace maplestory.io.Services.MapleStory
{
    public class CraftingEffectFactory : ICraftingEffectFactory
    {
        private readonly IWZFactory _factory;
        private readonly Dictionary<CraftingType, FrameBook> effects;

        public CraftingEffectFactory(IWZFactory wzFactory)
        {
            _factory = wzFactory;

            WZObject effectWz = wzFactory.GetWZFile(WZ.Effect).MainDirectory;
            WZObject container = effectWz.ResolvePath("CharacterEff.img/MeisterEff");

            string[] names = Enum.GetNames(typeof(CraftingType));

            effects = names.ToDictionary(c => (CraftingType)Enum.Parse(typeof(CraftingType), c, true), c => FrameBook.ParseSingle(effectWz, container, container.ResolvePath(c)));
        }

        public string[] EffectList() => effects.Keys.Select(c => c.ToString()).ToArray();

        public FrameBook GetEffect(CraftingType crafting) => effects[crafting];

        public FrameBook GetEffect(string crafting) => effects[(CraftingType)Enum.Parse(typeof(CraftingType), crafting, true)];
    }
}
