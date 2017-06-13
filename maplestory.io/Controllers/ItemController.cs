using maplestory.io.Services.MapleStory;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WZData;
using WZData.MapleStory.Items;
using System.Linq;
using WZData.MapleStory.Images;
using ImageSharp;
using System;

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
            resolver.Ignore<ItemType>(a => a.HighItemId);
            resolver.Ignore<ItemType>(a => a.LowItemId);

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

        [Route("category/{overallCategory}")]
        [HttpGet]
        [ProducesResponseType(typeof(ItemNameInfo), 200)]
        public IActionResult ListByCategory(string overallCategory)
            => Json(itemFactory.GetItems().Where(c => c.TypeInfo.OverallCategory.Equals(overallCategory, StringComparison.CurrentCultureIgnoreCase)), serializerSettings);

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
            if (eq == null) return NotFound("Couldn't find the item");

            if (eq.MetaInfo.Icon?.Icon != null)
                return File(eq.MetaInfo.Icon.Icon.ImageToByte(), "image/png");
            else if (eq is Equip)
            {
                Equip eqp = (Equip)eq;
                EquipFrameBook book = eqp.FrameBooks.Select(c => c.Value).FirstOrDefault();
                Image<Rgba32> effectImage = book?.frames?.FirstOrDefault()?.Effects?.Select(c => c.Value)?.First()?.Image;
                if (effectImage != null)
                    return File(effectImage.ImageToByte(), "image/png");
            }
            return NotFound("Item does not have an icon or a default effect");
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