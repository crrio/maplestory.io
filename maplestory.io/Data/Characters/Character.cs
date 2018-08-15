using PKG1;

namespace maplestory.io.Data.Characters {
    public class Character {
        public AvatarItemEntry[] ItemEntries;
        public float Zoom;
        public string Name, AnimationName;
        public bool FlipX;
        public int Padding, FrameNumber;
        public RenderMode Mode;
    }

    public class AvatarItemEntry {
        public int ItemId;
        public float Hue, Brightness, Contract, Saturation;
        public string Version;
        public Region Region;
    }
}