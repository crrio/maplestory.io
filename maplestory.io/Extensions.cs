using System;
using System.Collections.Generic;
using ImageSharp;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace maplestory.io
{
    public static class Extensions
    {
        public static byte[] ImageToByte(this Image<Rgba32> img)
        {
            using (MemoryStream mem = new MemoryStream())
            {
                img.Save(mem, ImageFormats.Png);
                return mem.ToArray();
            }
        }
    }
}
