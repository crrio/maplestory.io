using System;
using ImageSharp;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using PKG1;

namespace WZData.MapleStory.Images
{
    public interface IPositionedFrameContainer
    {
        Frame Canvas { get; }
        Vector3 Position { get; }
        RectangleF Bounds { get; }
    }
}