
using MoreLinq;
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
using WZData;
using SixLabors.Primitives;
using WZData.MapleStory.Images;
using SixLabors.ImageSharp;

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

        [FromQuery] public bool showEars { get; set; } = false;
        [FromQuery] public bool showLefEars { get; set; } = false;
        [FromQuery] public int padding { get; set; } = 2;
        [FromQuery] public string name { get; set; } = null;
        [FromQuery] public float resize { get; set; } = 1;
        [FromQuery] public bool flipX { get; set; } = false;

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
            => File(_factory.GetWithWZ(region, version).GetBase(skinId).ImageToByte(Request), "image/png");

        [Route("base/{skinId?}/example")]
        [HttpGet]
        [Produces("image/png")]
        public IActionResult GetBaseExample(int skinId = 2000)
            => File(_factory.GetWithWZ(region, version).GetBaseWithHair(skinId).ImageToByte(Request), "image/png");

        [Route("{skinId}/{items?}/{animation?}/{frame?}")]
        [HttpGet]
        [Produces("image/png")]
        public IActionResult GetCharacter(int skinId, string items = "1102039", string animation = null, int frame = 0, [FromQuery] RenderMode renderMode = RenderMode.Full)
            => File(_factory.GetWithWZ(region, version).GetCharacter(skinId, animation, frame, showEars, showLefEars, padding, name, resize, flipX, renderMode, items
                    .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Split(':', ';'))
                    .Where(c => c.Length > 0 && int.TryParse(c[0], out int blah))
                    .Select(c => new Tuple<int, string, float?>(int.Parse(c[0]), c.Length > 1 && !float.TryParse(c[1], out float blah) ? c[1] : animation, c.Length > 2 && float.TryParse(c[2], out float huehuehue) ? (float?)huehuehue : (c.Length > 1 && float.TryParse(c[1], out huehuehue) ? (float?)huehuehue : null)))
                    .OrderBy(c => c.Item1, OrderByDirection.Descending)
                    .ToArray()
                ).ImageToByte(Request, false), "image/png");

        [Route("compact/{skinId}/{items?}/{animation?}/{frame?}")]
        [HttpGet]
        [Produces("image/png")]
        public IActionResult GetCompactCharacter(int skinId, string items = "1102039", string animation = null, int frame = 0, [FromQuery] bool showEars = false)
        => GetCharacter(skinId, items, animation, frame, RenderMode.Compact);

        [Route("json/{skinId}/{items?}/{animation?}/{frame?}")]
        [HttpGet]
        [ProducesResponseType(typeof(Tuple<Frame, Point>[]), 200)]
        public IActionResult GetJsonCharacter(int skinId, string items = "1102039", string animation = null, int frame = 0)
            => Json(_factory.GetWithWZ(region, version).GetJsonCharacter(skinId, animation, frame, showEars, showLefEars, padding, name, resize, flipX, items
                    .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Split(':', ';'))
                    .Where(c => c.Length > 0 && int.TryParse(c[0], out int blah))
                    .Select(c => new Tuple<int, string, float?>(int.Parse(c[0]), c.Length > 1 && !float.TryParse(c[1], out float blah) ? c[1] : animation, c.Length > 2 && float.TryParse(c[2], out float huehuehue) ? (float?)huehuehue : (c.Length > 1 && float.TryParse(c[1], out huehuehue) ? (float?)huehuehue : null)))
                    .OrderBy(c => c.Item1, OrderByDirection.Descending)
                    .ToArray()));
        [Route("center/{skinId}/{items?}/{animation?}/{frame?}")]
        [HttpGet]
        [Produces("image/png")]
        public IActionResult GetCenteredCharacter(int skinId, string items = "1102039", string animation = null, int frame = 0)
            => GetCharacter(skinId, items, animation, frame, RenderMode.Centered);

        [Route("navelCenter/{skinId}/{items?}/{animation?}/{frame?}")]
        [HttpGet]
        [Produces("image/png")]
        public IActionResult GetNavelCenteredCharacter(int skinId, string items = "1102039", string animation = null, int frame = 0)
            => GetCharacter(skinId, items, animation, frame, RenderMode.NavelCenter);

        [Route("feetCenter/{skinId}/{items?}/{animation?}/{frame?}")]
        [HttpGet]
        [Produces("image/png")]
        public IActionResult GetFeetCenteredCharacter(int skinId, string items = "1102039", string animation = null, int frame = 0)
            => GetCharacter(skinId, items, animation, frame, RenderMode.FeetCenter);

        [Route("detailed/{skinId}/{items?}/{animation?}/{frame?}")]
        [HttpGet]
        [Produces(typeof(Tuple<Image<Rgba32>, Dictionary<string, Point>>))]
        public IActionResult GetCharacterDetails(int skinId, string items, string animation, int frame, RenderMode renderMode, bool showEars, bool showLefEars, int padding)
        {
            Tuple<Image<Rgba32>, Dictionary<string, Point>, Dictionary<string, int>, int> detailed = _factory.GetWithWZ(region, version).GetDetailedCharacter(skinId, animation, frame, showEars, showLefEars, padding, name, resize, flipX, renderMode, items
                .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Split(':', ';'))
                .Where(c => c.Length > 0 && int.TryParse(c[0], out int blah))
                .Select(c => new Tuple<int, string, float?>(int.Parse(c[0]), c.Length > 1 && !float.TryParse(c[1], out float blah) ? c[1] : animation, c.Length > 2 && float.TryParse(c[2], out float huehuehue) ? (float?)huehuehue : (c.Length > 1 && float.TryParse(c[1], out huehuehue) ? (float?)huehuehue : null)))
                .OrderBy(c => c.Item1, OrderByDirection.Descending)
                .ToArray()
            );
            return Json(new Tuple<byte[], Dictionary<string, Point>, Dictionary<string, int>, int>(detailed.Item1.ImageToByte(Request, false), detailed.Item2, detailed.Item3, detailed.Item4));
        }

        [Route("animated/{skinId}/{items?}/{animation?}/{frame?}")]
        [HttpGet]
        [Produces(typeof(Tuple<Image<Rgba32>, Dictionary<string, Point>>))]
        public IActionResult GetCharacterAnimated(int skinId, string items, string animation, RenderMode renderMode, bool showEars, bool showLefEars, int padding, [FromQuery] string bgColor = "")
        {
            Rgba32 background = Rgba32.Transparent;

            if (!string.IsNullOrEmpty(bgColor))
            {
                string[] bgColorNumbers = bgColor.Split(',');
                float[] rgb = bgColorNumbers.Take(3).Select(c => byte.Parse(c) / (byte.MaxValue * 1f)).ToArray();
                float alpha = float.Parse(bgColorNumbers[3]);
                background = new Rgba32(rgb[0], rgb[1], rgb[2], alpha);
            }

            Tuple<int, string, float?>[] itemData = items
                .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Split(':', ';'))
                .Where(c => c.Length > 0 && int.TryParse(c[0], out int blah))
                .Select(c => new Tuple<int, string, float?>(int.Parse(c[0]), c.Length > 1 && !float.TryParse(c[1], out float blah) ? c[1] : animation, c.Length > 2 && float.TryParse(c[2], out float huehuehue) ? (float?)huehuehue : (c.Length > 1 && float.TryParse(c[1], out huehuehue) ? (float?)huehuehue : null)))
                .OrderBy(c => c.Item1, OrderByDirection.Descending)
                .ToArray();
            ICharacterFactory factory = _factory.GetWithWZ(region, version);

            Tuple<Image<Rgba32>, Dictionary<string, Point>, Dictionary<string, int>, int> firstFrame = factory.GetDetailedCharacter(skinId, animation, 0, showEars, showLefEars, padding, name, resize, flipX, renderMode, itemData);

            int animationFrames = firstFrame.Item3[animation];
            Tuple<Image<Rgba32>, Dictionary<string, Point>, Dictionary<string, int>, int>[] frames = new Tuple<Image<Rgba32>, Dictionary<string, Point>, Dictionary<string, int>, int>[animationFrames];
            frames[0] = firstFrame;
            for (int i = 1; i < animationFrames; ++i) frames[i] = factory.GetDetailedCharacter(skinId, animation, i, showEars, showLefEars, padding, name, resize, flipX, renderMode, itemData);

            // Idle positions 
            if (animation.Equals("alert", StringComparison.CurrentCultureIgnoreCase) || animation.StartsWith("stand", StringComparison.CurrentCultureIgnoreCase))
                frames = frames.Concat(MoreEnumerable.SkipLast(frames.Reverse().Skip(1), 1)).ToArray();

            Image<Rgba32>[] frameImages = frames.Select(c => c.Item1).ToArray();
            Point maxFeetCenter = new Point(
                frames.Select(c => c.Item2["feetCenter"].X).Max(),
                frames.Select(c => c.Item2["feetCenter"].Y).Max()
            );
            Point maxDifference = new Point(
                maxFeetCenter.X - frames.Select(c => c.Item2["feetCenter"].X).Min(),
                maxFeetCenter.Y - frames.Select(c => c.Item2["feetCenter"].Y).Min()
            );
            Image<Rgba32> gif = new Image<Rgba32>(frameImages.Select(c => c.Width).Max() + maxDifference.X, frameImages.Select(c => c.Height).Max() + maxDifference.Y);

            for (int i = 0; i < frames.Length; ++i)
            {
                Image<Rgba32> frameImage = frames[i].Item1;
                Point feetCenter = frames[i].Item2["feetCenter"];
                Point offset = new Point(0, maxFeetCenter.Y - feetCenter.Y);

                if (offset.X != 0 || offset.Y != 0)
                {
                    Image<Rgba32> offsetFrameImage = new Image<Rgba32>(gif.Width, gif.Height);
                    offsetFrameImage.Mutate(x =>
                    {
                        x.DrawImage(frameImage, 1, new Size(frameImage.Width, frameImage.Height), offset);
                    });
                    frameImage = offsetFrameImage;
                }

                if (frameImage.Width != gif.Width || frameImage.Height != gif.Height) frameImage.Mutate(x =>
                {
                    x.Crop(gif.Width, gif.Height);
                });

                if (background.A != 0)
                {
                    Image<Rgba32> frameWithBackground = new Image<Rgba32>(frameImage.Width, frameImage.Height);
                    frameWithBackground.Mutate(x =>
                    {
                        x.Fill(background);
                        x.DrawImage(frameImage, 1, new Size(frameImage.Width, frameImage.Height), Point.Empty);
                    });
                    frameImage = frameWithBackground;
                }

                ImageFrame<Rgba32> resultFrame = gif.Frames.AddFrame(frameImage.Frames.RootFrame);
                resultFrame.MetaData.FrameDelay = frames[i].Item4 / 10;
                resultFrame.MetaData.DisposalMethod = SixLabors.ImageSharp.Formats.Gif.DisposalMethod.RestoreToBackground;
            }
            gif.Frames.RemoveFrame(0);

            return File(gif.ImageToByte(Request, false, ImageFormats.Gif), "image/gif");
        }

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

            return File(_factory.GetWithWZ(region, version).GetCharacter(skinSelected, null, 0, false, false, 2, null, 1, false, RenderMode.Full,
                    itemIds.Concat(new int[] { face, hair })
                    .Select(c => new Tuple<int, string, float?>(c, null, null))
                    .ToArray()
                ).ImageToByte(Request), "image/png");
        }

        [Route("download/{skinId}/{items?}")]
        [HttpGet]
        [Produces("application/zip")]
        public IActionResult GetSpritesheet(int skinId, string items = "1102039", [FromQuery] RenderMode renderMode = RenderMode.Full, [FromQuery] bool showEars = false, [FromQuery] bool showLefEars = false, [FromQuery] int padding = 2, [FromQuery] SpriteSheetFormat format = SpriteSheetFormat.Plain)
            => File(_factory.GetWithWZ(region, version).GetSpriteSheet(Request, skinId, showEars, showLefEars, padding, renderMode, format, items
                .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Split(';'))
                .Select(c => new Tuple<int, float?>(int.Parse(c[0]), c.Length > 1 && float.TryParse(c[1], out float huehuehue) ? (float?)huehuehue : null))
                .ToArray()), "application/zip", "CharacterSpriteSheet.zip");
    }
}