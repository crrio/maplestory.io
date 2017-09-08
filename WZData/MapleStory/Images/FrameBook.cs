using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using ImageSharp;
using PKG1;

namespace WZData
{
    public class FrameBook
    {
        public IEnumerable<Frame> frames;

        public static IEnumerable<FrameBook> Parse(WZProperty self)
        {
            if (self == null) return null;

            bool isSingle = self.Children.Any(c => c.Value.Type == PropertyType.Canvas);

            if (!isSingle)
                return self.Children
                    .Select(d => ParseSingle(d.Value))
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
                    return int.TryParse(c.Key, out frameNumber);
                })
                .OrderBy(c => int.Parse(c.Key))
                .Select(c => Frame.Parse(c.Value));

            return effect;
        }

        public static int GetFrameCount(WZProperty self)
        {
            return self.Children
                .Where(c =>
                {
                    int frameNumber = -1;
                    return int.TryParse(c.Key, out frameNumber);
                })
                .OrderBy(c => int.Parse(c.Key)).Count();
        }
    }
}