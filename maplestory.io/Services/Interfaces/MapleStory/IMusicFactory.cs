﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;

namespace maplestory.io.Services.Interfaces.MapleStory
{
    public interface IMusicFactory
    {
        byte[] GetSong(string songPath);
        string[] GetSounds();
        bool DoesSoundExist(string path);
    }
}