using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using System.IO;

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
                    img = img.Clone(c => c.Resize(new SixLabors.ImageSharp.Processing.ResizeOptions()
                    {
                        Mode = SixLabors.ImageSharp.Processing.ResizeMode.Stretch,
                        Sampler = new SixLabors.ImageSharp.Processing.NearestNeighborResampler(),
                        Size = new SixLabors.Primitives.Size((int)(img.Width * resizeAmount), (int)(img.Height * resizeAmount))
                    }));
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
