using System;
using System.Linq;
using reWZ.WZProperties;
using System.Collections.Generic;
using System.Diagnostics;
using ImageSharp;

namespace WZData
{
    public class FrameBook
    {
        public IEnumerable<Frame> frames;

        public static IEnumerable<FrameBook> Parse(WZObject file, WZObject container, WZObject self)
        {
            bool isSingle = self.Any(c => c is WZCanvasProperty);

            if (!isSingle)
            {
                return self
                    .Select(d => ParseSingle(file, container, d))
                    .Where(d => d.frames.Count() > 0);
            }
            else
            {
                return new FrameBook[] { ParseSingle(file, container, self) };
            }
        }

        public static FrameBook ParseSingle(WZObject file, WZObject container, WZObject self)
        {
            FrameBook effect = new FrameBook();

            effect.frames = self
                .Where(c =>
                {
                    int frameNumber = -1;
                    return int.TryParse(c.Name, out frameNumber);
                })
                .OrderBy(c => int.Parse(c.Name))
                .Select(frame =>
                {
                    return Frame.Parse(file, container, frame);
                });

            return effect;
        }
    }
}