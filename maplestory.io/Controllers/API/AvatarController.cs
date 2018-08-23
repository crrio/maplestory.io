using maplestory.io.Data.Characters;
using maplestory.io.Services.Implementations.MapleStory;
using maplestory.io.Services.Interfaces.MapleStory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MoreLinq;
using Newtonsoft.Json;
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
    [Route("api/character/{items}/{animation?}/{frame?}")]
    public class AvatarController : APIController
    {
        [FromQuery] public bool showEars { get; set; } = false;
        [FromQuery] public bool showLefEars { get; set; } = false;
        [FromQuery] public int padding { get; set; } = 2;
        [FromQuery] public string name { get; set; } = null;
        [FromQuery] public float resize { get; set; } = 1;
        [FromQuery] public bool flipX { get; set; } = false;
        public string items = "{itemId:1102039}";
        public AvatarItemEntry[] itemEntries { get => items.Split(',').Select(c => JsonConvert.DeserializeObject<AvatarItemEntry>(c)).ToArray(); }
        public string animation = "stand1";
        public int frame = 0;
        public Character Character 
        {
            get => new Character() {
                AnimationName = animation,
                FrameNumber = frame,
                ItemEntries = itemEntries,
                FlipX = flipX,
                Padding = padding,
                Zoom = resize,
                Mode = RenderMode.Full
            };
        }
        private Random rng;
        private ILogger<WZFactory> _logging;
        public AvatarController(ILogger<WZFactory> logger)
        {
            _logging = logger;
            rng = new Random();
        }

        [Route("")]
        [HttpGet]
        public IActionResult Render([FromQuery] RenderMode renderMode = RenderMode.Full)
            => File(this.AvatarFactory.Render(Character).ImageToByte(Request), "image/png");

        [Route("detailed")]
        [HttpGet]
        public IActionResult GetCharacterDetails(int skinId, string items, string animation, int frame, RenderMode renderMode, bool showEars, bool showLefEars, int padding)
            => throw new NotImplementedException();

        [Route("animated")]
        [HttpGet]
        public IActionResult GetCharacterAnimated(int skinId, string items, string animation, RenderMode renderMode, bool showEars, bool showLefEars, int padding, [FromQuery] string bgColor = "")
            => throw new NotImplementedException();

        [Route("download")]
        [HttpGet]
        public IActionResult GetSpritesheet(int skinId, string items = "1102039", [FromQuery] RenderMode renderMode = RenderMode.Full, [FromQuery] bool showEars = false, [FromQuery] bool showLefEars = false, [FromQuery] int padding = 2, [FromQuery] SpriteSheetFormat format = SpriteSheetFormat.Plain)
            => throw new NotImplementedException();
    }
}