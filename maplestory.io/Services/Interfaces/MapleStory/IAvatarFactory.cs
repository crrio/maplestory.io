using System;
using maplestory.io.Data.Characters;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace maplestory.io.Services.Interfaces.MapleStory
{
    public interface IAvatarFactory
    {
        Image<Rgba32> Render(Character c);
    }
}
