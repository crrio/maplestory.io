using maplestory.io.Data;
using maplestory.io.Data.Characters;
using Microsoft.AspNetCore.Mvc;
using MoreLinq;
using PKG1;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace maplestory.io.Controllers.API
{
    [Route("api/{region}/{version}/chat")]
    public class ChatController : APIController
    {
        [Route("")]
        [HttpGet]
        public IActionResult GetChat([FromQuery]string ringIdsJoined, [FromQuery]string message)
        {
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

            Font font = CharacterAvatar.fonts.Families.First(f => f.Name.Equals("Arial", StringComparison.CurrentCultureIgnoreCase)).CreateFont(12, FontStyle.Regular);
            RendererOptions textOptions = new RendererOptions(font);

            string[] lines = message.Batch(15).Select(b => new string(b.ToArray())).ToArray();
            SizeF[] lineSizes = lines.Select(line => TextMeasurer.Measure(line, textOptions)).ToArray();

            int textWidth = (int)Math.Ceiling(lineSizes.Select(b => b.Width).Max());
            int width = c.Width + e.Width + n.Width + s.Width + ne.Width + nw.Width + se.Width + sw.Width + textWidth;
            int textHeight = (int)Math.Ceiling(lineSizes.Sum(b => b.Height)); ;
            int height = c.Height + e.Height + n.Height + s.Height + ne.Height + nw.Height + se.Height + sw.Height + arrow.Height + textHeight;
            Point center = new Point(width / 2, height / 2 - arrow.Height);
            Point nwPos = new Point(center.X - textWidth / 2, center.Y - textHeight / 2);
            Point nePos = new Point(center.X + textWidth / 2, center.Y - textHeight / 2);
            Point swPos = new Point(center.X - textWidth / 2, center.Y + textHeight / 2);
            swPos = new Point(swPos.X, swPos.Y + (swPos.Y % (sw.Height - swOrigin.Y)));
            Point sePos = new Point(center.X + textWidth / 2, center.Y + textHeight / 2);
            sePos = new Point(sePos.X, sePos.Y + (sePos.Y % (se.Height - seOrigin.Y)));
            Point arrowPos = new Point(center.X, center.Y + textHeight + s.Height - sOrigin.Y);
            Image<Rgba32> result = new Image<Rgba32>(width, height);

            Tuple<Image<Rgba32>, Point, Point>[] corners = new Tuple<Image<Rgba32>, Point, Point>[] {
                new Tuple<Image<Rgba32>, Point, Point>(nw, nwOrigin, nwPos),
                new  Tuple<Image<Rgba32>, Point, Point>(ne, neOrigin, nePos),
                new Tuple<Image<Rgba32>, Point, Point>(sw, swOrigin, swPos),
                new Tuple<Image<Rgba32>, Point, Point>(se, seOrigin, sePos)
            };

            Tuple<Image<Rgba32>, Point, Point, Point>[] sides = new Tuple<Image<Rgba32>, Point, Point, Point>[]
            {
                new Tuple<Image<Rgba32>, Point, Point, Point>(w, wOrigin, nwPos, swPos),
                new Tuple<Image<Rgba32>, Point, Point, Point>(e, eOrigin, nePos, sePos),
                new Tuple<Image<Rgba32>, Point, Point, Point>(n, nOrigin, nwPos, nePos),
                new Tuple<Image<Rgba32>, Point, Point, Point>(s, sOrigin, swPos, sePos),
            };

            result.Mutate(image =>
            {
                for (int x = nwPos.X - cOrigin.X; x < nePos.X; x += c.Width)
                    for (int y = nwPos.Y - cOrigin.X; y < swPos.Y; y += c.Height)
                        image.DrawImage(c, 1, new Size(c.Width, c.Height), new Point(x, y));
                foreach (var side in sides)
                {
                    for (int x = side.Item3.X - side.Item2.X; x <= side.Item4.X; x += (side.Item1.Width))
                    {
                        for (int y = side.Item3.Y - side.Item2.Y; y <= side.Item4.Y; y += (side.Item1.Height))
                        {
                            image.DrawImage(side.Item1, 1, new Size(side.Item1.Width, side.Item1.Height), new Point(x, y));
                            if (side.Item3.Y == side.Item4.Y) break;
                        }
                        if (side.Item3.X == side.Item4.X) break;
                    }
                }
                foreach (var corner in corners) image.DrawImage(corner.Item1, 1, new Size(corner.Item1.Width, corner.Item1.Height), new Point(corner.Item3.X - corner.Item2.X, corner.Item3.Y - corner.Item2.Y));
                for (int i = 0; i < lines.Length; ++i)
                {
                    int x = center.X - ((int)lineSizes[i].Width / 2);
                    int y = (int)lineSizes.Take(i).Select(b => b.Height).DefaultIfEmpty(0).Sum(b => b) + (center.Y - (textHeight / 2));
                    image.DrawText(lines[i], font, nameColor, new Point(x, y));
                }

                image.DrawImage(arrow, 1, new Size(arrow.Width, arrow.Height), new Point(center.X - arrowOrigin.X, swPos.Y - arrowOrigin.Y));
            });

            return File(result.ImageToByte(Request), "image/png");
        }
    }
}
