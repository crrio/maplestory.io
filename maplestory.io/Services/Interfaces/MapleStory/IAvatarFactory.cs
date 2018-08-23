using System;
using System.Collections.Generic;
using maplestory.io.Data.Characters;
using Microsoft.AspNetCore.Http;
using PKG1;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;

namespace maplestory.io.Services.Interfaces.MapleStory
{
    public interface IAvatarFactory
    {
        Image<Rgba32> Render(Character c);
        byte[] GetSpriteSheet(HttpRequest request, SpriteSheetFormat format, Character character);
        Tuple<Image<Rgba32>, Dictionary<string, Point>, Dictionary<string, int>, int> Details(Character character);
        Dictionary<string, int> GetBodyFrameCounts(WZProperty bodyNode);
        Dictionary<string, int> GetPossibleActions(AvatarItemEntry[] items);
        Image<Rgba32> Animate(Character character, Rgba32? background = null);
    }
}
