using maplestory.io.Services.MapleStory;
using Microsoft.AspNetCore.Mvc;
using WZData;

namespace maplestory.io.Controllers
{
    [Produces("application/json")]
    [Route("api/item")]
    public class ItemController : Controller
    {
        private readonly IItemFactory itemFactory;

        public ItemController(IItemFactory factory)
        {
            itemFactory = factory;
        }

        [Route("list")]
        public IActionResult List() => Json(itemFactory.GetItems());

        [Route("{itemId}")]
        public IActionResult itemSearch(int itemId)
        {
            MapleItem eq = itemFactory.search(itemId);
            return Json(eq);
        }

        [Route("{itemId}/icon")]
        public IActionResult itemIcon(int itemId)
        {
            MapleItem eq = itemFactory.search(itemId);
            return File(eq.MetaInfo.Icon.Icon.ImageToByte(), "image/png");
        }
        [Route("{itemId}/iconRaw")]

        public IActionResult itemIconRaw(int itemId)
        {
            MapleItem eq = itemFactory.search(itemId);
            return File(eq.MetaInfo.Icon.IconRaw.ImageToByte(), "image/png");
        }
    }
}