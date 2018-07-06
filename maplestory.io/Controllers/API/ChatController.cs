using maplestory.io.Data.Characters;
using Microsoft.AspNetCore.Mvc;
using MoreLinq;
using PKG1;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Drawing;
using SixLabors.ImageSharp.Processing.Text;
using SixLabors.Primitives;
using System;
using System.Linq;

namespace maplestory.io.Controllers.API
{
    [Route("api/{region}/{version}/chat")]
    public class ChatController : APIController
    {
        [Route("")]
        [HttpGet]
        public IActionResult GetChat([FromQuery]string ringIdsJoined, [FromQuery]string message)
        {
            if (message.Length > 512) throw new InvalidOperationException("Too long of a message");

            int[] ringIds = ringIdsJoined.Split(',').Select(b => int.TryParse(b, out int d) ? (int?)d : null).Where(b => b.HasValue).Select(b => b.Value).ToArray();
            WZProperty rings = WZ.Resolve("Character/Ring");
            int chatBalloonID = ringIds.Select(b => rings.ResolveFor<int>($"{b.ToString("D8")}.img/info/chatBalloon")).Where(b => b.HasValue).Select(b => b.Value).FirstOrDefault();
            WZProperty chatBalloon = WZ.Resolve($"UI/ChatBalloon/{chatBalloonID}");

            Image<Rgba32> c = null, e = null, n = null, ne = null, nw = null, s = null, se = null, sw = null, w = null, arrow = null;
            Point cOrigin = Point.Empty, eOrigin = Point.Empty, nOrigin = Point.Empty, neOrigin = Point.Empty, nwOrigin = Point.Empty, sOrigin = Point.Empty, seOrigin = Point.Empty, swOrigin = Point.Empty, wOrigin = Point.Empty, arrowOrigin = Point.Empty;
            int color = 0;
            foreach(WZProperty prop in chatBalloon.Children)
            {
                if (prop.Name == "c")
                {
                    c = prop.ResolveForOrNull<Image<Rgba32>>();
                    cOrigin = prop.ResolveFor<Point>("origin") ?? Point.Empty;
                }
                if (prop.Name == "e")
                {
                    e = prop.ResolveForOrNull<Image<Rgba32>>();
                    eOrigin = prop.ResolveFor<Point>("origin") ?? Point.Empty;
                }
                if (prop.Name == "n")
                {
                    n = prop.ResolveForOrNull<Image<Rgba32>>();
                    nOrigin = prop.ResolveFor<Point>("origin") ?? Point.Empty;
                }
                if (prop.Name == "w")
                {
                    w = prop.ResolveForOrNull<Image<Rgba32>>();
                    wOrigin = prop.ResolveFor<Point>("origin") ?? Point.Empty;
                }
                if (prop.Name == "s")
                {
                    s = prop.ResolveForOrNull<Image<Rgba32>>();
                    sOrigin = prop.ResolveFor<Point>("origin") ?? Point.Empty;
                }
                if (prop.Name == "ne")
                {
                    ne = prop.ResolveForOrNull<Image<Rgba32>>();
                    neOrigin = prop.ResolveFor<Point>("origin") ?? Point.Empty;
                }
                if (prop.Name == "nw")
                {
                    nw = prop.ResolveForOrNull<Image<Rgba32>>();
                    nwOrigin = prop.ResolveFor<Point>("origin") ?? Point.Empty;
                }
                if (prop.Name == "se")
                {
                    se = prop.ResolveForOrNull<Image<Rgba32>>();
                    seOrigin = prop.ResolveFor<Point>("origin") ?? Point.Empty;
                }
                if (prop.Name == "sw")
                {
                    sw = prop.ResolveForOrNull<Image<Rgba32>>();
                    swOrigin = prop.ResolveFor<Point>("origin") ?? Point.Empty;
                }
                if (prop.Name == "arrow")
                {
                    arrow = prop.ResolveForOrNull<Image<Rgba32>>();
                    arrowOrigin = prop.ResolveFor<Point>("origin") ?? Point.Empty;
                }
                if (prop.Name == "clr") color = prop.ResolveFor<int>() ?? 0;
            }

            Rgba32 nameColor = new Rgba32();
            new Argb32((uint)color).ToRgba32(ref nameColor);

            Font font = CharacterAvatar.fonts.Families.First(f => f.Name.Equals("Arial Unicode MS", StringComparison.CurrentCultureIgnoreCase)).CreateFont(12, FontStyle.Regular);
            RendererOptions textOptions = new RendererOptions(font);

            string[] lines = message.Batch(32).Select(b => new string(b.ToArray())).ToArray();
            SizeF[] lineSizes = lines.Select(line => TextMeasurer.Measure(line, textOptions)).ToArray();

            int leftPadding = new[] { nw.Width, w.Width, sw.Width }.Max();
            int topPadding = new[] { ne.Height, n.Height, nw.Height }.Max();
            int rightPadding = new[] { ne.Width, e.Width, se.Width }.Max();
            int bottomPadding = new[] { se.Height, s.Height, sw.Height }.Max() + arrow.Height;

            int textWidth = (int)Math.Ceiling(lineSizes.Select(b => b.Width).Max());
            textWidth = textWidth + (c.Width - (textWidth % c.Width));
            if (textWidth < c.Width * 2) textWidth = c.Width * 2;
            int width = leftPadding + rightPadding + textWidth;
            int textHeight = (int)Math.Ceiling(lineSizes.Sum(b => b.Height));
            int adjustTextY = (c.Height - (textHeight % c.Height));
            textHeight = textHeight + adjustTextY;
            if (textHeight < c.Height * 2) textHeight = c.Height * 2;
            int height = topPadding + bottomPadding + textHeight;
            Point center = new Point(width / 2, (height - arrow.Height) / 2);

            Point nwPos = new Point((leftPadding - nw.Width) + nwOrigin.X, nwOrigin.Y);
            Point nePos = new Point(neOrigin.X + width - rightPadding, neOrigin.Y);
            Point swPos = new Point((leftPadding - sw.Width) + swOrigin.X, swOrigin.Y + height - bottomPadding);
            Point sePos = new Point(seOrigin.X + width - rightPadding, seOrigin.Y + height - bottomPadding);
            Point arrowPos = new Point(center.X, arrowOrigin.Y + height - arrow.Height);
            Image<Rgba32> result = new Image<Rgba32>(width, height);

            Tuple<Image<Rgba32>, Point, Point>[] corners = new Tuple<Image<Rgba32>, Point, Point>[] {
                new Tuple<Image<Rgba32>, Point, Point>(nw, nwOrigin, nwPos),
                new  Tuple<Image<Rgba32>, Point, Point>(ne, neOrigin, nePos),
                new Tuple<Image<Rgba32>, Point, Point>(sw, swOrigin, swPos),
                new Tuple<Image<Rgba32>, Point, Point>(se, seOrigin, sePos)
            };

            Tuple<Image<Rgba32>, Point, Point, Point>[] sides = new Tuple<Image<Rgba32>, Point, Point, Point>[]
            {
                new Tuple<Image<Rgba32>, Point, Point, Point>(w, wOrigin, new Point(leftPadding, nw.Height), new Point(leftPadding, swPos.Y - swOrigin.Y)),
                new Tuple<Image<Rgba32>, Point, Point, Point>(e, eOrigin, new Point(width - rightPadding, ne.Height), new Point(width - rightPadding, sePos.Y - seOrigin.Y)),
                new Tuple<Image<Rgba32>, Point, Point, Point>(n, nOrigin, new Point(nw.Width, topPadding), new Point(width - rightPadding, topPadding)),
                new Tuple<Image<Rgba32>, Point, Point, Point>(s, sOrigin, new Point((swPos.X + sw.Width - swOrigin.X), height - bottomPadding), new Point(width - rightPadding, height - bottomPadding))
            };

            result.Mutate(image =>
            {
                for (int x = leftPadding; x < textWidth + leftPadding; x += c.Width)
                    for (int y = topPadding; y < textHeight + topPadding; y += c.Height)
                        image.DrawImage(c, 1, new Point(x, y));
                foreach (var side in sides)
                {
                    for (int x = side.Item3.X; x <= side.Item4.X; x += (side.Item1.Width))
                    {
                        for (int y = side.Item3.Y; y <= side.Item4.Y; y += (side.Item1.Height))
                        {
                            image.DrawImage(side.Item1, 1, new Point(x - side.Item2.X, y - side.Item2.Y));
                            if (side.Item3.Y == side.Item4.Y || (y + side.Item1.Height >= side.Item4.Y)) break;
                        }
                        if (side.Item3.X == side.Item4.X || (x + side.Item1.Width >= side.Item4.X)) break;
                    }
                }
                foreach (var corner in corners) image.DrawImage(corner.Item1, PixelBlenderMode.Src, 1, new Point(corner.Item3.X - corner.Item2.X, corner.Item3.Y - corner.Item2.Y));
                for (int i = 0; i < lines.Length; ++i)
                {
                    int x = center.X - ((int)lineSizes[i].Width / 2);
                    int y = (int)lineSizes.Take(i).Select(b => b.Height).DefaultIfEmpty(0).Sum(b => b) + ((height - arrow.Height) - textHeight) / 2;
                    image.DrawText(lines[i], font, nameColor, new Point(x, y + adjustTextY / 2));
                }

                image.DrawImage(arrow, 1, new Point(center.X - arrowOrigin.X, swPos.Y - arrowOrigin.Y));
            });

            return File(result.ImageToByte(Request), "image/png");
        }
    }
}
