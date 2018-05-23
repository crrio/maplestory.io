using System;
using System.Collections.Generic;
using System.Linq;
using PKG1;
using maplestory.io.Services.Interfaces.MapleStory;

namespace maplestory.io.Services.Implementations.MapleStory
{
    public class MusicFactory : NeedWZ, IMusicFactory
    {
        IEnumerable<string> RecursiveGetSoundPath(IEnumerable<WZProperty> directory)
        {
            foreach (WZProperty obj in directory)
            {
                if (obj?.Children == null) continue;
                if (obj.Children.Count() > 0)
                    foreach (string s in RecursiveGetSoundPath(obj.Children)) yield return s;
                if (obj.Type == PropertyType.Audio)
                    yield return obj.Path.Substring(6).Replace(".img", "");
            }
        }

        public byte[] GetSong(string songPath)
            => WZ.Resolve("Sound").ResolveForOrNull<byte[]>($"{songPath.Trim('/', ' ', '\\').Replace(".img", "")}");
        public string[] GetSounds(int startPosition = 0, int? count = null)
            => RecursiveGetSoundPath(WZ.Resolve("Sound").Children).Skip(startPosition).Take(count ?? int.MaxValue).ToArray();

        public bool DoesSoundExist(string songPath)
            => WZ.Resolve("Sound").Resolve($"{songPath.Trim('/', ' ', '\\').Replace(".img", "")}")?.Type == PropertyType.Audio;
    }
}
