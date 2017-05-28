using System;
using System.Linq;
using reWZ.WZProperties;
using System.Collections.Generic;

namespace WZData.MapleStory.Images
{
    public class EquipFrameBook
    {
        public IEnumerable<EquipFrame> frames;
        internal static EquipFrameBook Parse(WZObject skills, WZObject skill, WZObject frameContainer)
        {
            EquipFrameBook effect = new EquipFrameBook();

            bool isSingle = frameContainer.Any(c => c is WZCanvasProperty);

            if (!isSingle)
            {
                effect.frames = frameContainer
                .Where(c =>
                {
                    int frameNumber = -1;
                    return int.TryParse(c.Name, out frameNumber);
                })
                .OrderBy(c =>
                {
                    int frameNumber = -1;
                    if (int.TryParse(c.Name, out frameNumber)) return frameNumber;
                    return 1;
                })
                .Select(frame =>
                {
                    return EquipFrame.Parse(skills, skill, frame);
                }).ToArray();
            }
            else
            {
                effect.frames = new EquipFrame[] { EquipFrame.Parse(skills, skill, frameContainer) };
            }

            return effect;
        }
    }
}
