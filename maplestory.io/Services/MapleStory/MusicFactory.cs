using System;
using System.Collections.Generic;
using System.Linq;
using PKG1;

namespace maplestory.io.Services.MapleStory
{
    public class MusicFactory : NeedWZ<IMusicFactory>, IMusicFactory
    {
        public MusicFactory(IWZFactory factory) : base(factory) { }
        public MusicFactory(IWZFactory factory, Region region, string version) : base(factory, region, version) { }

        IEnumerable<string> RecursiveGetSoundPath(IEnumerable<WZProperty> directory)
        {
            foreach (WZProperty obj in directory)
            {
                if (obj?.Children == null) continue;
                if (obj.Children.Count > 0)
                    foreach (string s in RecursiveGetSoundPath(obj.Children.Values)) yield return s;
                if (obj.Type == PropertyType.Audio)
                    yield return obj.Path.Substring(6).Replace(".img", "");
            }
        }

        public byte[] GetSong(string songPath)
            => wz.Resolve("Sound").ResolveForOrNull<byte[]>($"{songPath.Trim('/', ' ', '\\').Replace(".img", "")}");
        public string[] GetSounds()
            => RecursiveGetSoundPath(wz.Resolve("Sound").Children.Values).ToArray();

        public bool DoesSoundExist(string songPath)
            => wz.Resolve("Sound").Resolve($"{songPath.Trim('/', ' ', '\\').Replace(".img", "")}")?.Type == PropertyType.Audio;

        public override IMusicFactory GetWithWZ(Region region, string version)
            => new MusicFactory(_factory, region, version);
    }
}
