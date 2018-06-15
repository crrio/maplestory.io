using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.MetaData;
using SixLabors.ImageSharp.MetaData.Profiles.Exif;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System.IO;
using System.Threading;

namespace maplestory.io
{
    public static class Extensions
    {
        static PngEncoder pngEncoder;
        static GifEncoder gifEncoder;

        static Extensions()
        {
            pngEncoder = new PngEncoder()
            {
                CompressionLevel = 1
            };
            gifEncoder = new GifEncoder() { };
        }

        public static byte[] ImageToByte(this Image<Rgba32> img, HttpRequest context, bool autoResize = true, IImageFormat format = null, bool autoDispose = false)
        {
            if (format == null) format = ImageFormats.Png;
            if (context.Query.ContainsKey("resize") && autoResize)
            {
                string userResizeAmount = context.Query["resize"];
                decimal resizeAmount = decimal.Parse(userResizeAmount);
                if (resizeAmount != 1 && (img.Height * resizeAmount) < 50000 && (img.Width * resizeAmount) < 50000)
                {
                    img = img.Clone(c => c.Resize(new ResizeOptions()
                    {
                        Mode = ResizeMode.Stretch,
                        Sampler = new NearestNeighborResampler(),
                        Size = new Size((int)(img.Width * resizeAmount), (int)(img.Height * resizeAmount))
                    }));
                }
            }

            using (MemoryStream mem = new MemoryStream())
            {
                if (img.MetaData.ExifProfile == null)
                {
                    img.MetaData.ExifProfile = new ExifProfile();
                    img.MetaData.ExifProfile.SetValue(ExifTag.Software, $"Generated at {"https://maplestory.io" + context.Path}");
                }
                if (format == ImageFormats.Png)
                    pngEncoder.Encode(img, mem);
                else if (format == ImageFormats.Gif)
                    gifEncoder.Encode(img, mem);
                else
                    img.Save(mem, format);

                if (autoDispose) ThreadPool.QueueUserWorkItem((s) => img.Dispose());
                return mem.ToArray();
            }
        }
    }
}
