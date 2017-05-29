using reWZ;
using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace maplestory.io.Services.MapleStory
{
    public class MusicFactory : IMusicFactory
    {
        public string[] allSounds;
        Dictionary<string, Func<byte[]>> soundLookup;
        public MusicFactory(IWZFactory factory)
        {
            WZFile sounds = factory.GetWZFile(WZ.Sound);
            allSounds = RecursiveGetSoundPath(sounds.MainDirectory).ToArray();
            soundLookup = new Dictionary<string, Func<byte[]>>();
            IEnumerable<Tuple<string, Func<byte[]>>> lookups = allSounds.Select(c => new Tuple<string, Func<byte[]>>(c.Trim('/', ' ', '\\').ToLower().Replace(".img", ""), () => ((WZAudioProperty)sounds.ResolvePath(c)).Value));
            foreach (Tuple<string, Func<byte[]>> lookup in lookups) soundLookup.Add(lookup.Item1, lookup.Item2);
        }

        IEnumerable<string> RecursiveGetSoundPath(IEnumerable<WZObject> directory)
        {
            foreach (WZObject obj in directory)
            {
                if (obj.ChildCount > 0)
                    foreach (string s in RecursiveGetSoundPath(obj)) yield return s;
                if (obj is WZAudioProperty)
                    yield return obj.Path;
            }
        }

        public byte[] GetSong(string songPath) => soundLookup[songPath.Trim('/', ' ', '\\').ToLower().Replace(".img", "")]();
        public string[] GetSounds() => soundLookup.Keys.ToArray();
        public bool DoesSoundExist(string path) => soundLookup.ContainsKey(path.Trim('/', ' ', '\\').ToLower().Replace(".img", ""));
    }
}
