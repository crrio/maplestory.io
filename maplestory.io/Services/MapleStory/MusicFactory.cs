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

        IEnumerable<string> RecursiveGetSoundPath(WZDirectory directory)
        {
            foreach (WZObject obj in directory)
            {
                if (obj is WZDirectory)
                    foreach (string s in RecursiveGetSoundPath((WZDirectory)obj)) yield return s;
                if (obj is WZImage)
                    foreach (string s in RecursiveGetSoundPath((WZImage)obj)) yield return s;
                if (obj is WZAudioProperty)
                    yield return obj.Path;
            }
        }

        IEnumerable<string> RecursiveGetSoundPath(WZImage directory)
        {
            foreach (WZObject obj in directory)
            {
                if (obj is WZDirectory)
                    foreach (string s in RecursiveGetSoundPath((WZDirectory)obj)) yield return s;
                if (obj is WZImage)
                    foreach (string s in RecursiveGetSoundPath((WZImage)obj)) yield return s;
                if (obj is WZAudioProperty)
                    yield return obj.Path;
            }
        }

        public byte[] GetSong(string songPath) => soundLookup[songPath.Trim('/', ' ', '\\').ToLower().Replace(".img", "")]();

        public string[] GetSounds() => allSounds;
    }
}
