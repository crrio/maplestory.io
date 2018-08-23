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
    [Route("api/character/{items}")]
    public class AvatarController : APIController
    {
        [FromQuery] public bool showEars { get; set; } = false;
        [FromQuery] public bool showLefEars { get; set; } = false;
        [FromQuery] public int padding { get; set; } = 2;
        [FromQuery] public string name { get; set; } = null;
        [FromQuery] public float resize { get; set; } = 1;
        [FromQuery] public bool flipX { get; set; } = false;
        [FromRoute] public string items { get; set; } = "{itemId:1102039}";
        [FromRoute] public string animation { get; set; } = "stand1";
        [FromRoute] public int frame { get; set; }
        [FromQuery] RenderMode renderMode { get; set; } = RenderMode.Full;

        public AvatarItemEntry[] itemEntries { get => JsonConvert.DeserializeObject<AvatarItemEntry[]>($"[{items}]"); }
        public Character Character 
        {
            get => new Character()
            {
                AnimationName = animation,
                FrameNumber = frame,
                ItemEntries = itemEntries,
                FlipX = flipX,
                Padding = padding,
                Zoom = resize,
                Mode = renderMode,
                ElfEars = showEars,
                LefEars = showLefEars,
                Name = name
            };
        }
        private Random rng;
        private ILogger<WZFactory> _logging;
        public AvatarController(ILogger<WZFactory> logger)
        {
            _logging = logger;
            rng = new Random();
        }

        [Route("detailed")]
        [HttpGet]
        public IActionResult GetCharacterDetails() => Json(this.AvatarFactory.Details(Character));

        [Route("animated")]
        [HttpGet]
        public IActionResult GetCharacterAnimated([FromQuery] string bgColor = "")
        {
            Rgba32 background = Rgba32.Transparent;

            if (!string.IsNullOrEmpty(bgColor))
            {
                string[] bgColorNumbers = bgColor.Split(',');
                float[] rgb = bgColorNumbers.Take(3).Select(c => byte.Parse(c) / (byte.MaxValue * 1f)).ToArray();
                float alpha = float.Parse(bgColorNumbers[3]);
                background = new Rgba32(rgb[0], rgb[1], rgb[2], alpha);
            }

            return File(this.AvatarFactory.Animate(Character, background).ImageToByte(Request, false, ImageFormats.Gif, true), "image/gif");
        }

        [Route("download")]
        [HttpGet]
        public IActionResult GetSpritesheet([FromQuery] SpriteSheetFormat format = SpriteSheetFormat.Plain) 
            => File(AvatarFactory.GetSpriteSheet(Request, format, Character), "application/zip", "CharacterSpriteSheet.zip");

        [Route("{animation?}/{frame?}")]
        [HttpGet]
        public IActionResult Render()
            => File(this.AvatarFactory.Render(Character).ImageToByte(Request), "image/png");
    }
}