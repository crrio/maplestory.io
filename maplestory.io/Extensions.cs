using System;
using System.Collections.Generic;
using ImageSharp;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace maplestory.io
{
    public static class Extensions
    {
        public static byte[] ImageToByte(this Image<Rgba32> img, HttpRequest context)
        {
            if (context.Query.ContainsKey("resize"))
            {
                string userResizeAmount = context.Query["resize"];
                decimal resizeAmount = decimal.Parse(userResizeAmount);
                if (resizeAmount != 1 && (img.Height * resizeAmount) < 50000 && (img.Width * resizeAmount) < 50000)
                {
                    img = (new Image<Rgba32>(img)).Resize(new ImageSharp.Processing.ResizeOptions()
                    {
                        Mode = ImageSharp.Processing.ResizeMode.Stretch,
                        Sampler = new ImageSharp.Processing.NearestNeighborResampler(),
                        Size = new SixLabors.Primitives.Size((int)(img.Width * resizeAmount), (int)(img.Height * resizeAmount))
                    });
                }
            }

            using (MemoryStream mem = new MemoryStream())
            {
                img.Save(mem, ImageFormats.Png);
                return mem.ToArray();
            }
        }
    }
}
