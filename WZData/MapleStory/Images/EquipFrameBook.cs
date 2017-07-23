﻿using System;
using System.Linq;
using System.Collections.Generic;
using PKG1;

namespace WZData.MapleStory.Images
{
    public class EquipFrameBook
    {
        public static Action<string> ErrorCallback = (s) => { };
        public IEnumerable<EquipFrame> frames;

        internal static EquipFrameBook Parse(WZProperty container)
        {
            EquipFrameBook effect = new EquipFrameBook();

            // If we are a UOL, resolve, otherwise it'll return itself
            container = container.Resolve();

            bool isSingle = container.Children.Any(c => c.Value.Type == PropertyType.Canvas);

            if (!isSingle)
            {
                effect.frames = container.Children
                .Where(c =>
                {
                    int frameNumber = -1;
                    return int.TryParse(c.Key, out frameNumber);
                })
                .OrderBy(c =>
                {
                    int frameNumber = -1;
                    if (int.TryParse(c.Key, out frameNumber)) return frameNumber;
                    return 1;
                })
                .Select(frame =>
                {
                    try {
                        return EquipFrame.Parse(frame.Value);
                    } catch (Exception ex) {
                        ErrorCallback($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                        return null;
                    }
                }).ToArray();
            }
            else
            {
                effect.frames = new EquipFrame[] { EquipFrame.Parse(container) };
            }

            return effect;
        }
    }
}
