using maplestory.io.Models;
using maplestory.io.Services.Implementations.MapleStory;
using maplestory.io.Services.Interfaces.MapleStory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using PKG1;

namespace maplestory.io.Controllers
{
    public abstract class APIController : Controller
    {
        [FromRoute(Name = "region")]
        public Region Region { get; set; }
        [FromRoute(Name = "version")]
        public string Version { get; set; }
        public MSPackageCollection WZ { get => WZFactory.GetWZ(Region, Version); }
        protected IWZFactory WZFactory { get => Request.HttpContext.RequestServices.GetService<IWZFactory>(); }
        protected IAndroidFactory AndroidFactory { get => GetWithWZ(Request.HttpContext.RequestServices.GetService<IAndroidFactory>()); }
        protected ICharacterFactory CharacterFactory { get => GetWithWZ(Request.HttpContext.RequestServices.GetService<ICharacterFactory>()); }
        protected ICraftingEffectFactory CraftingEffectFactory { get => GetWithWZ(Request.HttpContext.RequestServices.GetService<ICraftingEffectFactory>()); }
        protected IItemFactory ItemFactory { get => GetWithWZ(Request.HttpContext.RequestServices.GetService<IItemFactory>()); }
        protected IMapFactory MapFactory { get => GetWithWZ(Request.HttpContext.RequestServices.GetService<IMapFactory>()); }
        protected IMobFactory MobFactory { get => GetWithWZ(Request.HttpContext.RequestServices.GetService<IMobFactory>()); }
        protected IMusicFactory MusicFactory { get => GetWithWZ(Request.HttpContext.RequestServices.GetService<IMusicFactory>()); }
        protected INPCFactory NPCFactory { get => GetWithWZ(Request.HttpContext.RequestServices.GetService<INPCFactory>()); }
        protected IPetFactory PetFactory { get => GetWithWZ(Request.HttpContext.RequestServices.GetService<IPetFactory>()); }
        protected IQuestFactory QuestFactory { get => GetWithWZ(Request.HttpContext.RequestServices.GetService<IQuestFactory>()); }
        protected ISkillFactory SkillFactory { get => GetWithWZ(Request.HttpContext.RequestServices.GetService<ISkillFactory>()); }
        protected ITipFactory TipFactory { get => GetWithWZ(Request.HttpContext.RequestServices.GetService<ITipFactory>()); }
        protected IZMapFactory ZMapFactory { get => GetWithWZ(Request.HttpContext.RequestServices.GetService<IZMapFactory>()); }

        K GetWithWZ<K>(K that)
            where K : class
        {
            if (that is NeedWZ)
            {
                // Have to cast to object before we can actually cast to NeedsWZ. Thanks C#.
                NeedWZ needs = (NeedWZ)(object)that;
                needs.WZ = this.WZ;
                needs.Region = this.Region;
                needs.Version = this.Version;
            }
            return that;
        }
    }
}
