using maplestory.io.Data.Characters;
using maplestory.io.Services.Implementations.MapleStory;
using maplestory.io.Services.Interfaces.MapleStory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MoreLinq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Drawing;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.Primitives;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace maplestory.io.Controllers.API
{
    [Route("api/{region}/{version}/Character")]
    public class CharacterController : APIController
    {
        [FromQuery] public bool showEars { get; set; } = false;
        [FromQuery] public bool showLefEars { get; set; } = false;
        [FromQuery] public int padding { get; set; } = 2;
        [FromQuery] public string name { get; set; } = null;
        [FromQuery] public float resize { get; set; } = 1;
        [FromQuery] public bool flipX { get; set; } = false;

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

        public CharacterController(ILogger<WZFactory> logger)
        {
            _logging = logger;
            rng = new Random();
        }

        [Route("base/{skinId?}")]
        [HttpGet]
        public IActionResult GetBase(int skinId = 2000)
            => File(CharacterFactory.GetBase(skinId).ImageToByte(Request, true, null, true), "image/png");

        [Route("base/{skinId?}/example")]
        [HttpGet]
        public IActionResult GetBaseExample(int skinId = 2000)
            => File(CharacterFactory.GetBaseWithHair(skinId).ImageToByte(Request, true, null, true), "image/png");

        [Route("{skinId}/{items?}/{animation?}/{frame?}")]
        [HttpGet]
        public IActionResult GetCharacter(int skinId, string items = "1102039", string animation = null, int frame = 0, [FromQuery] RenderMode renderMode = RenderMode.Full)
            => File(CharacterFactory.GetCharacter(skinId, animation, frame, showEars, showLefEars, padding, name, resize, flipX, renderMode, items
                    .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Split(':', ';'))
                    .Where(c => c.Length > 0 && int.TryParse(c[0], out int blah))
                    .Select(c => new Tuple<int, string, float?>(int.Parse(c[0]), c.Length > 1 && !float.TryParse(c[1], out float blah) ? c[1] : animation, c.Length > 2 && float.TryParse(c[2], out float huehuehue) ? (float?)huehuehue : (c.Length > 1 && float.TryParse(c[1], out huehuehue) ? (float?)huehuehue : null)))
                    .OrderBy(c => c.Item1, OrderByDirection.Descending)
                    .ToArray()
                ).ImageToByte(Request, false, null, true), "image/png");

        [Route("compact/{skinId}/{items?}/{animation?}/{frame?}")]
        [HttpGet]
        public IActionResult GetCompactCharacter(int skinId, string items = "1102039", string animation = null, int frame = 0, [FromQuery] bool showEars = false)
        => GetCharacter(skinId, items, animation, frame, RenderMode.Compact);

        [Route("json/{skinId}/{items?}/{animation?}/{frame?}")]
        [HttpGet]
        public IActionResult GetJsonCharacter(int skinId, string items = "1102039", string animation = null, int frame = 0)
            => Json(CharacterFactory.GetJsonCharacter(skinId, animation, frame, showEars, showLefEars, padding, name, resize, flipX, items
                    .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Split(':', ';'))
                    .Where(c => c.Length > 0 && int.TryParse(c[0], out int blah))
                    .Select(c => new Tuple<int, string, float?>(int.Parse(c[0]), c.Length > 1 && !float.TryParse(c[1], out float blah) ? c[1] : animation, c.Length > 2 && float.TryParse(c[2], out float huehuehue) ? (float?)huehuehue : (c.Length > 1 && float.TryParse(c[1], out huehuehue) ? (float?)huehuehue : null)))
                    .OrderBy(c => c.Item1, OrderByDirection.Descending)
                    .ToArray()));
        [Route("center/{skinId}/{items?}/{animation?}/{frame?}")]
        [HttpGet]
        public IActionResult GetCenteredCharacter(int skinId, string items = "1102039", string animation = null, int frame = 0)
            => GetCharacter(skinId, items, animation, frame, RenderMode.Centered);

        [Route("navelCenter/{skinId}/{items?}/{animation?}/{frame?}")]
        [HttpGet]
        public IActionResult GetNavelCenteredCharacter(int skinId, string items = "1102039", string animation = null, int frame = 0)
            => GetCharacter(skinId, items, animation, frame, RenderMode.NavelCenter);

        [Route("feetCenter/{skinId}/{items?}/{animation?}/{frame?}")]
        [HttpGet]
        public IActionResult GetFeetCenteredCharacter(int skinId, string items = "1102039", string animation = null, int frame = 0)
            => GetCharacter(skinId, items, animation, frame, RenderMode.FeetCenter);

        [Route("detailed/{skinId}/{items?}/{animation?}/{frame?}")]
        [HttpGet]
        public IActionResult GetCharacterDetails(int skinId, string items, string animation, int frame, RenderMode renderMode, bool showEars, bool showLefEars, int padding)
        {
            Tuple<Image<Rgba32>, Dictionary<string, Point>, Dictionary<string, int>, int> detailed = CharacterFactory.GetDetailedCharacter(skinId, animation, frame, showEars, showLefEars, padding, name, resize, flipX, renderMode, items?
                .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Split(':', ';'))
                .Where(c => c.Length > 0 && int.TryParse(c[0], out int blah))
                .Select(c => new Tuple<int, string, float?>(int.Parse(c[0]), c.Length > 1 && !float.TryParse(c[1], out float blah) ? c[1] : animation, c.Length > 2 && float.TryParse(c[2], out float huehuehue) ? (float?)huehuehue : (c.Length > 1 && float.TryParse(c[1], out huehuehue) ? (float?)huehuehue : null)))
                .OrderBy(c => c.Item1, OrderByDirection.Descending)
                .ToArray()
            );
            return Json(new Tuple<byte[], Dictionary<string, Point>, Dictionary<string, int>, int>(detailed.Item1.ImageToByte(Request, false), detailed.Item2, detailed.Item3, detailed.Item4));
        }

        [Route("multidetailed/{skinId}/{items}")]
        [HttpGet]
        public IActionResult GetMultipleCharacterDetails(int skinId, string items, [FromQuery]string animations, [FromQuery]string faces)
        {
            string[] animationsSplit = animations.Split(',');
            string[] facesSplit = faces.Split(',');
            ConcurrentDictionary<string, Tuple<Image<Rgba32>, Dictionary<string, Point>, int>> allDetails = new ConcurrentDictionary<string, Tuple<Image<Rgba32>, Dictionary<string, Point>, int>>();
            List<Tuple<string, string>> allPossible = new List<Tuple<string, string>>();
            foreach (string animation in animationsSplit)
                foreach (string face in facesSplit)
                    allPossible.Add(new Tuple<string, string>(animation, face));
            Parallel.ForEach(allPossible, animationFaceTuple =>
            {
                string animation = animationFaceTuple.Item1;
                string face = animationFaceTuple.Item2;

                Tuple<int, string, float?>[] itemEntries = items?
                    .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Split(':', ';'))
                    .Where(c => c.Length > 0 && int.TryParse(c[0], out int blah))
                    .Select(c => {
                        int itemId = int.Parse(c[0]);
                        Tuple<int, string, float?> itemEntry = new Tuple<int, string, float?>(itemId, itemId >= 20000 && itemId < 30000 ? face : animation, null);
                        return itemEntry;
                    }).OrderBy(c => c.Item1, OrderByDirection.Descending).ToArray();

                int frame = 0;
                int frameCount = 0;
                do
                {
                    Tuple<Image<Rgba32>, Dictionary<string, Point>, Dictionary<string, int>, int> detailed = CharacterFactory.GetDetailedCharacter(skinId, animation, frame, showEars, showLefEars, padding, name, resize, flipX, RenderMode.Full, itemEntries);
                    allDetails.TryAdd($"{animation}-{face}-{frame}", new Tuple<Image<Rgba32>, Dictionary<string, Point>, int>(detailed.Item1, detailed.Item2, detailed.Item4));

                    frameCount = detailed.Item3[animation];
                    frame++;
                } while (frame < frameCount);
            });

            return Json(allDetails);
        }

        [Route("animated/{skinId}/{items?}/{animation?}/{frame?}")]
        [HttpGet]
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
            ICharacterFactory factory = CharacterFactory;

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

            List<Image<Rgba32>> pendingDispose = new List<Image<Rgba32>>();

            for (int i = 0; i < frames.Length; ++i)
            {
                Image<Rgba32> frameImage = frames[i].Item1;
                Point feetCenter = frames[i].Item2["feetCenter"];
                Point offset = new Point(maxFeetCenter.X - feetCenter.X, maxFeetCenter.Y - feetCenter.Y);

                if (offset.X != 0 || offset.Y != 0)
                {
                    Image<Rgba32> offsetFrameImage = new Image<Rgba32>(gif.Width, gif.Height);
                    offsetFrameImage.Mutate(x =>
                    {
                        x.DrawImage(frameImage, 1, offset);
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
                        x.DrawImage(frameImage, 1, Point.Empty);
                    });

                    if (frameImage != frames[i].Item1) frameImage.Dispose();
                    frameImage = frameWithBackground;
                }

                ImageFrame<Rgba32> resultFrame = gif.Frames.AddFrame(frameImage.Frames.RootFrame);
                resultFrame.MetaData.FrameDelay = frames[i].Item4 / 10;
                resultFrame.MetaData.DisposalMethod = SixLabors.ImageSharp.Formats.Gif.DisposalMethod.RestoreToBackground;

                pendingDispose.Add(frameImage);
                if (frameImage != frames[i].Item1) pendingDispose.Add(frames[i].Item1);
            }
            gif.Frames.RemoveFrame(0);

            pendingDispose.ForEach(c => c.Dispose());

            return File(gif.ImageToByte(Request, false, ImageFormats.Gif, true), "image/gif");
        }

        [Route("actions/{items?}")]
        [HttpGet]
        public IActionResult GetPossibleActions(string items = "1102039")
            => Json(CharacterFactory.GetActions(items
                .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Split(':'))
                .Where(c => c.Length > 0 && int.TryParse(c[0], out int blah))
                .Select(c => int.Parse(c[0]))
                .ToArray()));

        [Route("")]
        [HttpGet]
        public IActionResult GetSkinTypes() => Json(CharacterFactory.GetSkinIds());

        [Route("random")]
        [HttpGet]
        public IActionResult GetRandomCharacter()
        {
            int[] itemIds = presets[rng.Next(0, presets.Length - 1)];
            int skinSelected = skinIds[rng.Next(0, skinIds.Length - 1)];
            int hair = hairIds[rng.Next(0, hairIds.Length)];
            int face = faceIds[rng.Next(0, faceIds.Length)];

            _logging.LogInformation("Generating random character with: {0}", string.Join(",", itemIds.Concat(new int[] { face, hair })));

            return File(CharacterFactory.GetCharacter(skinSelected, null, 0, false, false, 2, null, 1, false, RenderMode.Full,
                    itemIds.Concat(new int[] { face, hair })
                    .Select(c => new Tuple<int, string, float?>(c, null, null))
                    .ToArray()
                ).ImageToByte(Request, true, null, true), "image/png");
        }

        [Route("download/{skinId}/{items?}")]
        [HttpGet]
        public IActionResult GetSpritesheet(int skinId, string items = "1102039", [FromQuery] RenderMode renderMode = RenderMode.Full, [FromQuery] bool showEars = false, [FromQuery] bool showLefEars = false, [FromQuery] int padding = 2, [FromQuery] SpriteSheetFormat format = SpriteSheetFormat.Plain)
            => File(CharacterFactory.GetSpriteSheet(Request, skinId, showEars, showLefEars, padding, renderMode, format, items
                .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Split(';'))
                .Select(c => new Tuple<int, float?>(int.Parse(c[0]), c.Length > 1 && float.TryParse(c[1], out float huehuehue) ? (float?)huehuehue : null))
                .ToArray()), "application/zip", "CharacterSpriteSheet.zip");
    }
}