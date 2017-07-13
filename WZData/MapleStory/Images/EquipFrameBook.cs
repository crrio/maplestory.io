using System;
using System.Linq;
using reWZ.WZProperties;
using System.Collections.Generic;

namespace WZData.MapleStory.Images
{
    public class EquipFrameBook
    {
        public static Action<string> ErrorCallback = (s) => { };
        public IEnumerable<EquipFrame> frames;
        internal static EquipFrameBook Parse(WZObject skills, WZObject skill, WZObject frameContainer)
        {
            EquipFrameBook effect = new EquipFrameBook();

            if (frameContainer.Type == WZObjectType.UOL) frameContainer = ((WZUOLProperty)frameContainer).ResolveFully();

            bool isSingle = frameContainer.Any(c => c.Type == WZObjectType.Canvas);

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
                    try {
                        return EquipFrame.Parse(skills, skill, frame);
                    } catch (Exception ex) {
                        ErrorCallback($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                        return null;
                    }
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
