using maplestory.io.Data.Characters;
using Microsoft.AspNetCore.Mvc;
using PKG1;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Drawing;
using SixLabors.ImageSharp.Processing.Text;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.Primitives;
using SixLabors.Shapes;
using System;
using System.Linq;

namespace maplestory.io.Controllers.API
{
    [Route("api/{region}/{version}/name")]
    public class NameTagController : APIController
    {
        [Route("")]
        [HttpGet]
        public IActionResult GetNameTag([FromQuery]string Name, [FromQuery]string ringIdsJoined= "")
        {
            if (Name.Length > 64) Name = Name.Substring(0, 64);

            int[] ringIds = ringIdsJoined.Split(',').Select(b => int.TryParse(b, out int d) ? (int?)d : null).Where(b => b.HasValue).Select(b => b.Value).ToArray();
            WZProperty rings = WZ.Resolve("Character/Ring");
            int? chatBalloonID = ringIds.Select(b => rings.ResolveFor<int>($"{b.ToString("D8")}.img/info/nameTag")).Where(b => b.HasValue).Select(b => b.Value).FirstOrDefault();

            WZProperty nameTag = null;
            if (chatBalloonID.HasValue)
                nameTag = WZ.Resolve($"UI/NameTag/{chatBalloonID.Value}");

            Image<Rgba32> c = nameTag?.ResolveForOrNull<Image<Rgba32>>("c");
            Point cOrigin = nameTag?.ResolveFor<Point>("c/origin") ?? Point.Empty;
            Image<Rgba32> w = nameTag?.ResolveForOrNull<Image<Rgba32>>("w");
            Point wOrigin = nameTag?.ResolveFor<Point>("w/origin") ?? Point.Empty;
            Image<Rgba32> e = nameTag?.ResolveForOrNull<Image<Rgba32>>("e");
            Point eOrigin = nameTag?.ResolveFor<Point>("e/origin") ?? Point.Empty;
            int nameColorVal = nameTag?.ResolveFor<int>("clr") ?? -1;
            Rgba32 nameColor = new Rgba32();
            new Argb32((uint)nameColorVal).ToRgba32(ref nameColor);

            Font MaplestoryFont = CharacterAvatar.fonts.Families
                .First(f => f.Name.Equals("Arial Unicode MS", StringComparison.CurrentCultureIgnoreCase)).CreateFont(12, FontStyle.Regular);
            SizeF realNameSize = TextMeasurer.Measure(Name, new RendererOptions(MaplestoryFont));

            Rectangle nameTagSize = new Rectangle(0, 0, (int)realNameSize.Width, (int)realNameSize.Height);
            int startY = Math.Max(c?.Height ?? 0, Math.Max(w?.Height ?? 0, e?.Height ?? 0));
            if (nameTag != null) nameTagSize = new Rectangle(0, 0, nameTagSize.Width + w.Width + e.Width - eOrigin.X, nameTagSize.Height + startY);
            else nameTagSize = new Rectangle(0, 0, nameTagSize.Width + 8, nameTagSize.Height + 8);

            Image<Rgba32> withName = new Image<Rgba32>(nameTagSize.Width + 4, nameTagSize.Height + 4);

            withName.Mutate(x =>
            {
                if (nameTag == null)
                {
                    x.Fill(new Rgba32(0, 0, 0, 128), nameTagSize);
                    IPathCollection iPath = BuildCorners(0, 0, nameTagSize.Width, nameTagSize.Height, 4);
                    x.Fill(new Rgba32(0, 0, 0, 0), iPath);
                    x.DrawText(Name, MaplestoryFont, nameColor, new PointF(4, 2));
                }
                else
                {
                    x.DrawImage(w, 1, new Point(0, startY - wOrigin.Y));
                    using (var cv = c.Clone(v => v.Resize(new Size((int)realNameSize.Width, c.Height))))
                        x.DrawImage(cv, 1, new Point(w.Width, startY - cOrigin.Y));
                    x.DrawImage(e, 1, new Point((int)(nameTagSize.Width - (e.Width - eOrigin.X)), startY - eOrigin.Y));
                    x.DrawText(Name, MaplestoryFont, nameColor, new PointF(w.Width, startY + 2 - cOrigin.Y));
                }
            });

            return File(withName.ImageToByte(Request), "image/png");
        }

        IPathCollection BuildCorners(int x, int y, int width, int height, float cornerRadius)
        {
            // first create a square
            var rect = new RectangularPolygon(x - 0.5f, y - 0.5f, cornerRadius, cornerRadius);

            // then cut out of the square a circle so we are left with a corner
            IPath cornerToptLeft = rect.Clip(new EllipsePolygon(x + (cornerRadius - 0.5f), y + (cornerRadius - 0.5f), cornerRadius));

            // corner is now a corner shape positions top left
            //lets make 3 more positioned correctly, we can do that by translating the orgional artound the center of the image
            var center = new PointF(width / 2F, height / 2F);

            float rightPos = width - cornerToptLeft.Bounds.Width + 1;
            float bottomPos = height - cornerToptLeft.Bounds.Height + 1;

            // move it across the widthof the image - the width of the shape
            IPath cornerTopRight = cornerToptLeft.RotateDegree(90).Translate(rightPos, 0);
            IPath cornerBottomLeft = cornerToptLeft.RotateDegree(-90).Translate(0, bottomPos);
            IPath cornerBottomRight = cornerToptLeft.RotateDegree(180).Translate(rightPos, bottomPos);

            return new PathCollection(cornerToptLeft, cornerBottomLeft, cornerTopRight, cornerBottomRight);
        }
    }
}
