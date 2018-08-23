using maplestory.io.Data.Images;
using PKG1;
using SixLabors.Primitives;

namespace maplestory.io.Data.Characters {
    public class Character {
        public Character() { }
        public Character(Character old)
        {
            this.ItemEntries = old.ItemEntries;
            this.Zoom = old.Zoom;
            this.Name = old.Name;
            this.AnimationName = old.AnimationName;
            this.FlipX = old.FlipX;
            this.Padding = old.Padding;
            this.FrameNumber = old.FrameNumber;
            this.Mode = old.Mode;
            this.ElfEars = old.ElfEars;
            this.LefEars = old.LefEars;
        }

        public AvatarItemEntry[] ItemEntries;
        public float Zoom;
        public string Name, AnimationName = "stand1";
        public bool FlipX;
        public int Padding, FrameNumber;
        public RenderMode Mode;
        public bool ElfEars;
        public bool LefEars;
    }

    public class AvatarItemEntry {
        public AvatarItemEntry() { }
        public AvatarItemEntry(AvatarItemEntry old)
        {
            this.ItemId = old.ItemId;
            this.Hue = old.Hue;
            this.Brightness = old.Brightness;
            this.Contrast = old.Contrast;
            this.Saturation = old.Saturation;
            this.Alpha = old.Alpha;
            this.Version = old.Version;
            this.Region = old.Region;
            this.AnimationName = old.AnimationName;
            this.EquipFrame = old.EquipFrame;
        }
        public int ItemId;
        public float? Hue, Brightness, Contrast, Saturation, Alpha;
        public string Version;
        public Region Region;
        public string AnimationName;
        public int? EquipFrame;
    }

    public class RankedFrame<K>
        where K : class
    {
        public readonly Frame frame;
        public readonly int ranking;
        public K underlyingEquip;

        public RankedFrame(Frame frame, int ranking, K underlyingEquip)
        {
            this.frame = frame;
            this.ranking = ranking;
            this.underlyingEquip = underlyingEquip;
        }
    }

    public class PositionedFrame
    {
        public readonly Frame frame;
        public readonly Point position;

        public PositionedFrame(Frame frame, Point position)
        {
            this.frame = frame;
            this.position = position;
        }
    }

    public enum RenderMode
    {
        Full,
        Compact,
        Centered,
        NavelCenter,
        FeetCenter
    }

    public enum SpriteSheetFormat
    {
        Plain = 0,
        PDNZip = 1,
        Minimal = 2
    }
}