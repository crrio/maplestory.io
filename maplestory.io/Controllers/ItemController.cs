using maplestory.io.Services.MapleStory;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WZData;
using WZData.MapleStory.Items;

namespace maplestory.io.Controllers
{
    [Produces("application/json")]
    [Route("api/item")]
    public class ItemController : Controller
    {
        private readonly IItemFactory itemFactory;
        private readonly JsonSerializerSettings serializerSettings;

        public ItemController(IItemFactory factory)
        { 
            itemFactory = factory;

            IgnorableSerializerContractResolver resolver = new IgnorableSerializerContractResolver();
            resolver.Ignore<ItemNameInfo>(a => a.Info);

            serializerSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = resolver,
                Formatting = Formatting.Indented
            };
        }

        [Route("list")]
        [Route("")]
        [HttpGet]
        [ProducesResponseType(typeof(ItemNameInfo), 200)]
        public IActionResult List() 
            => Json(itemFactory.GetItems(), serializerSettings);

        [Route("full")]
        [HttpGet]
        [ProducesResponseType(typeof(ItemNameInfo), 200)]
        public IActionResult FullList() => Json(itemFactory.GetItems());

        [Route("{itemId}")]
        [HttpGet]
        [ProducesResponseType(typeof(MapleItem), 200)]
        public IActionResult itemSearch(int itemId)
        {
            MapleItem eq = itemFactory.search(itemId);
            return Json(eq);
        }

        [Route("{itemId}/icon")]
        [HttpGet]
        [Produces("image/png")]
        public IActionResult itemIcon(int itemId)
        {
            MapleItem eq = itemFactory.search(itemId);
            return File(eq.MetaInfo.Icon.Icon.ImageToByte(), "image/png");
        }

        [Route("{itemId}/iconRaw")]
        [HttpGet]
        [Produces("image/png")]
        public IActionResult itemIconRaw(int itemId)
        {
            MapleItem eq = itemFactory.search(itemId);
            return File(eq.MetaInfo.Icon.IconRaw.ImageToByte(), "image/png");
        }
    }
}