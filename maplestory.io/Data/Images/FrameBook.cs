using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using SixLabors.ImageSharp;
using PKG1;

namespace maplestory.io.Data.Images
{
    public class FrameBook
    {
        public IEnumerable<Frame> frames;

        public static IEnumerable<FrameBook> Parse(WZProperty self)
        {
            if (self == null) return null;

            bool isSingle = self.Children.Any(c => c.Type == PropertyType.Canvas);

            if (!isSingle)
                return self.Children
                    .Select(d => ParseSingle(d))
                    .Where(d => d.frames.Count() > 0);
            else
                return new FrameBook[] { ParseSingle(self) };
        }

        public static FrameBook ParseSingle(WZProperty self)
        {
            FrameBook effect = new FrameBook();

            effect.frames = self.Children
                .Where(c =>
                {
                    int frameNumber = -1;
                    return int.TryParse(c.NameWithoutExtension, out frameNumber);
                })
                .OrderBy(c => int.Parse(c.NameWithoutExtension))
                .Select(c => Frame.Parse(c));

            return effect;
        }

        public static int GetFrameCount(WZProperty self)
        {
            return self.Children
                .Where(c =>
                {
                    int frameNumber = -1;
                    return int.TryParse(c.NameWithoutExtension, out frameNumber);
                })
                .OrderBy(c => int.Parse(c.NameWithoutExtension)).Count();
        }
    }
}