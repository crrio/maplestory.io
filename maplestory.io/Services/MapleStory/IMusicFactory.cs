using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace maplestory.io.Services.MapleStory
{
    public interface IMusicFactory
    {
        byte[] GetSong(string songPath);
        string[] GetSounds();
    }
}
