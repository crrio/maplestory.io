using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace maplestory.io
{
    public static class Extensions
    {
        public static byte[] ImageToByte(this Image<Rgba32> img, HttpRequest context, bool autoResize = true, IImageFormat format = null, bool autoDispose = false)
        {
            if (format == null) format = ImageFormats.Png;
            if (context.Query.ContainsKey("resize") && autoResize)
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
                img.Save(mem, format);
                if (autoDispose) ThreadPool.QueueUserWorkItem((s) => img.Dispose());
                return mem.ToArray();
            }
        }
    }
}
