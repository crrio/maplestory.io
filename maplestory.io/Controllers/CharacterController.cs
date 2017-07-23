using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using maplestory.io.Services.MapleStory;
using Microsoft.Extensions.Logging;
using PKG1;
using WZData.MapleStory.Characters;

namespace maplestory.io.Controllers
{
    [Produces("application/json")]
    [Route("api/{region}/{version}/Character")]
    public class CharacterController : Controller
    {
        [FromRoute]
        public Region region { get; set; }
        [FromRoute]
        public string version { get; set; }
        private ICharacterFactory _factory;
        private readonly IItemFactory _itemFactory;
        private Random rng;
        private ILogger<WZFactory> _logging;
        static readonly int[] hero1h = new int[] { 1302275, 1092113, 1003797, 1042254, 1062165, 1072743, 1082543, 1102481, 1132174, 1190301 };
        static readonly int[] aran = new int[] { 1442223, 1352932, 1003797, 1042254, 1062165, 1072743, 1082543, 1102481, 1132174, 1190521 };
        static readonly int[] bishop = new int[] { 1372177, 1092079, 1003798, 1042255, 1062166, 1072743, 1082543, 1102481, 1132174, 1190301 };
        static readonly int[] luminous = new int[] { 1212063, 1352403, 1003798, 1042255, 1062166, 1072743, 1082543, 1102481, 1132174, 1190521 };
        static readonly int[] marksman = new int[] { 1462193, 1352272, 1003799, 1042256, 1062167, 1072743, 1082543, 1102481, 1132174, 1190301 };
        static readonly int[] wildhunter = new int[] { 1462193, 1352962, 1003799, 1042256, 1062167, 1072743, 1082543, 1102481, 1132174, 1190601 };
        static readonly int[] cannoneer = new int[] { 1532098, 1352922, 1003801, 1042258, 1062169, 1072743, 1082543, 1102481, 1132174, 1190301 };
        static readonly int[] phantom = new int[] { 1362090, 1352103, 1003800, 1042257, 1062168, 1072743, 1082543, 1102481, 1132174, 1190521 };
        static readonly int[] xenon = new int[] { 1242060, 1353004, 1003801, 1042258, 1062169, 1072743, 1082543, 1102481, 1132174, 1190201 };
        /// <summary>
        /// These presets above have been pulled from https://github.com/stripedypaper/stripedypaper.github.io/tree/master/cube
        /// </summary>

        static readonly int[] beginner = new int[] { 1060002, 1072005, 1040002, 1302000 };
        static readonly int[] NXPerson = new int[] { 1062055, 1002943, 1072005, 1042078, 1302000 };
        static readonly int[][] presets = new int[][] { beginner, NXPerson, hero1h, aran, bishop, luminous, marksman, wildhunter, cannoneer, phantom, xenon };

        static readonly int[] hairIds = new int[] { 35350, 30830, 30800, 30330, 30120, 31220, 34120, 34240, 34560, 37950, 38006 };
        static readonly int[] faceIds = new int[] { 20008, 20023, 20104, 20214, 20315, 20425, 20588, 20699, 23563, 21273, 21657 };
        static readonly int[] skinIds = new int[] { 2000, 2001, 2002, 2003, 2004, 2005, 2009, 2010, 2011, 2012, 2013 };

        public CharacterController(ICharacterFactory factory, IItemFactory items, ILogger<WZFactory> logger)
        {
            _factory = factory;
            _itemFactory = items;
            _logging = logger;
            rng = new Random();
        }

        [Route("base/{skinId?}")]
        [HttpGet]
        [Produces("image/png")]
        public IActionResult GetBase(int skinId = 2000)
            => File(_factory.GetWithWZ(region, version).GetBase(skinId).ImageToByte(), "image/png");

        [Route("base/{skinId?}/example")]
        [HttpGet]
        [Produces("image/png")]
        public IActionResult GetBaseExample(int skinId = 2000)
            => File(_factory.GetWithWZ(region, version).GetBaseWithHair(skinId).ImageToByte(), "image/png");

        [Route("{skinId}/{items?}/{animation?}/{frame?}")]
        [HttpGet]
        [Produces("image/png")]
        public IActionResult GetCharacter(int skinId, string items = "1102039", string animation = null, int frame = 0, [FromQuery] RenderMode renderMode = RenderMode.Full, [FromQuery] bool showEars = false, [FromQuery] int padding = 2)
            => File(_factory.GetWithWZ(region, version).GetCharacter(skinId, animation, frame, showEars, padding, renderMode, items
                    .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Split(':'))
                    .Where(c => c.Length > 0 && int.TryParse(c[0], out int blah))
                    .Select(c => new Tuple<int, string>(int.Parse(c[0]), c.Length > 1 ? c[1] : animation))
                    .ToArray()
                ).ImageToByte(), "image/png");

        [Route("compact/{skinId}/{items?}/{animation?}/{frame?}")]
        [HttpGet]
        [Produces("image/png")]
        public IActionResult GetCompactCharacter(int skinId, string items = "1102039", string animation = null, int frame = 0, [FromQuery] bool showEars = false, [FromQuery] int padding = 2)
        => GetCharacter(skinId, items, animation, frame, RenderMode.Compact, showEars, padding);

        [Route("center/{skinId}/{items?}/{animation?}/{frame?}")]
        [HttpGet]
        [Produces("image/png")]
        public IActionResult GetCenteredCharacter(int skinId, string items = "1102039", string animation = null, int frame = 0, [FromQuery] bool showEars = false, [FromQuery] int padding = 2)
            => GetCharacter(skinId, items, animation, frame, RenderMode.Centered, showEars, padding);

        [Route("actions/{items?}")]
        [HttpGet]
        [ProducesResponseType(typeof(string[]), 200)]
        public IActionResult GetPossibleActions(string items = "1102039")
            => Json(_factory.GetWithWZ(region, version).GetActions(items
                .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Split(':'))
                .Where(c => c.Length > 0 && int.TryParse(c[0], out int blah))
                .Select(c => int.Parse(c[0]))
                .ToArray()));

        [Route("")]
        [HttpGet]
        [ProducesResponseType(typeof(int[]), 200)]
        public IActionResult GetSkinTypes() => Json(_factory.GetWithWZ(region, version).GetSkinIds());

        [Route("random")]
        [HttpGet]
        [Produces("image/png")]
        public IActionResult GetRandomCharacter()
        {
            int[] itemIds = presets[rng.Next(0, presets.Length - 1)];
            int skinSelected = skinIds[rng.Next(0, skinIds.Length - 1)];
            int hair = hairIds[rng.Next(0, hairIds.Length)];
            int face = faceIds[rng.Next(0, faceIds.Length)];

            _logging.LogInformation("Generating random character with: {0}", string.Join(",", itemIds.Concat(new int[] { face, hair })));

            return File(_factory.GetWithWZ(region, version).GetCharacter(skinSelected, null, 0, false, 2, RenderMode.Full,
                    itemIds.Concat(new int[] { face, hair })
                    .Select(c => new Tuple<int, string>(c, null))
                    .ToArray()
                ).ImageToByte(), "image/png");
        }

        [Route("download/{skinId}/{items?}")]
        [HttpGet]
        [Produces("application/zip")]
        public IActionResult GetSpritesheet(int skinId, string items = "1102039", [FromQuery] RenderMode renderMode = RenderMode.Full, [FromQuery] bool showEars = false, [FromQuery] int padding = 2)
            => File(_factory.GetWithWZ(region, version).GetSpriteSheet(skinId, showEars, padding, renderMode, items
                .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(c => int.Parse(c))
                .ToArray()), "application/zip", "CharacterSpriteSheet.zip");
    }
}