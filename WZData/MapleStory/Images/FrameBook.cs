using System;
using System.Linq;
using reWZ.WZProperties;
using System.Collections.Generic;
using System.Diagnostics;

namespace WZData
{
    public class FrameBook
    {
        public IEnumerable<Frame> frames;
        internal static IEnumerable<FrameBook> Parse(WZObject skills, WZObject skill, WZObject frameContainer)
        {
            bool isSingle = frameContainer.Any(c => c is WZCanvasProperty);

            if (!isSingle)
            {
                return frameContainer
                    .Select(d => ParseSingle(skills, skill, d))
                    .Where(d => d.frames.Count() > 0);
            }
            else
            {
                return new FrameBook[] { ParseSingle(skills, skill, frameContainer) };
            }
        }

        private static FrameBook ParseSingle(WZObject skills, WZObject skill, WZObject frameContainer)
        {
            FrameBook effect = new FrameBook();

            effect.frames = frameContainer
                .Where(c =>
                {
                    int frameNumber = -1;
                    return int.TryParse(c.Name, out frameNumber);
                })
                .OrderBy(c => int.Parse(c.Name))
                .Select(frame =>
                {
                    return Frame.Parse(skills, skill, frame);
                });

            return effect;
        }
    }
}