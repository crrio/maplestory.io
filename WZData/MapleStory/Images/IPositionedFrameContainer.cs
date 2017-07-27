using System;
using System.Numerics;
using SixLabors.Primitives;

namespace WZData.MapleStory.Images
{
    public interface IPositionedFrameContainer
    {
        Frame Canvas { get; }
        Vector3 Position { get; }
        RectangleF Bounds { get; }
        bool Flip { get; }
    }
}