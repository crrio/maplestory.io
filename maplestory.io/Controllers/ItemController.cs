using maplestory.io.Services.MapleStory;
using Microsoft.AspNetCore.Mvc;
using WZData;
using WZData.MapleStory.Items;

namespace maplestory.io.Controllers
{
    [Produces("application/json")]
    [Route("api/item")]
    public class ItemController : Controller
    {
        private readonly IItemFactory itemFactory;

        public ItemController(IItemFactory factory)
            => itemFactory = factory;

        [Route("list")]
        [Route("")]
        [HttpGet]
        [ProducesResponseType(typeof(ItemName), 200)]
        public IActionResult List() => Json(itemFactory.GetItems());

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