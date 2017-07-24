using maplestory.io.Services.MapleStory;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WZData;
using WZData.MapleStory.Items;
using System.Linq;
using WZData.MapleStory.Images;
using ImageSharp;
using System;
using PKG1;

namespace maplestory.io.Controllers
{
    [Produces("application/json")]
    [Route("api/{region}/{version}/item")]
    public class ItemController : Controller
    {
        private readonly IItemFactory itemFactory;
        private readonly JsonSerializerSettings serializerSettings;

        public ItemController(IItemFactory factory)
        {
            itemFactory = factory;

            IgnorableSerializerContractResolver resolver = new IgnorableSerializerContractResolver();
            resolver.Ignore<ItemNameInfo>(a => a.Info);
            resolver.Ignore<ItemType>(a => a.HighItemId);
            resolver.Ignore<ItemType>(a => a.LowItemId);

            serializerSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = resolver,
                Formatting = Formatting.Indented
            };
        }

        [FromRoute]
        public Region region { get; set; }
        [FromRoute]
        public string version { get; set; }
        [Route("list")]
        [Route("")]
        [HttpGet]
        [ProducesResponseType(typeof(ItemNameInfo[]), 200)]
        public IActionResult List()
            => Json(itemFactory.GetWithWZ(region, version).GetItems(), serializerSettings);

        [Route("full")]
        [HttpGet]
        [ProducesResponseType(typeof(ItemNameInfo[]), 200)]
        public IActionResult FullList() => Json(itemFactory.GetWithWZ(region, version).GetItems());

        [Route("category")]
        [HttpGet]
        [ProducesResponseType(typeof(string[]), 200)]
        public IActionResult GetCategories() => Json(itemFactory.GetWithWZ(region, version).GetItemCategories());

        [Route("category/{overallCategory}")]
        [HttpGet]
        [ProducesResponseType(typeof(ItemNameInfo), 200)]
        public IActionResult ListByCategory(string overallCategory)
            => Json(itemFactory.GetWithWZ(region, version).GetItems().Where(c => c.TypeInfo.OverallCategory.Equals(overallCategory, StringComparison.CurrentCultureIgnoreCase)), serializerSettings);

        [Route("{itemId}")]
        [HttpGet]
        [ProducesResponseType(typeof(MapleItem), 200)]
        public IActionResult itemSearch(int itemId)
        {
            MapleItem eq = itemFactory.GetWithWZ(region, version).search(itemId);
            return Json(eq);
        }

        [Route("{itemId}/icon")]
        [HttpGet]
        [Produces("image/png")]
        public IActionResult itemIcon(int itemId)
        {
            Image<Rgba32> icon = null;
            if (itemFactory.GetWithWZ(region, version).DoesItemExist(itemId) && (icon = itemFactory.GetWithWZ(region, version).GetIcon(itemId)) != null)
                return File(icon.ImageToByte(), "image/png");
            return NotFound("Item does not have an icon or a default effect");
        }

        [Route("{itemId}/iconRaw")]
        [HttpGet]
        [Produces("image/png")]
        public IActionResult itemIconRaw(int itemId)
        {
            Image<Rgba32> icon = null;
            if (itemFactory.GetWithWZ(region, version).DoesItemExist(itemId) && (icon = itemFactory.GetWithWZ(region, version).GetIconRaw(itemId)) != null)
                return File(icon.ImageToByte(), "image/png");
            return NotFound("Item does not have an icon or a default effect");
        }

        [Route("{itemId}/name")]
        [HttpGet]
        [Produces("text/json")]
        public IActionResult itemName(int itemId)
        {
            MapleItem eq = itemFactory.GetWithWZ(region, version).GetWithWZ(region, version).search(itemId);
            return Json(eq.Description);
        }
    }
}