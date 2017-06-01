using System;
using reWZ.WZProperties;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace WZData
{
    public class Frame
    {
        public Bitmap image;
        public int delay;
        public Point origin;
        public Dictionary<string, Point> MapOffset;
        public string Position;

        internal static Frame Parse(WZObject file, WZObject container, WZObject self)
        {
            Frame animationFrame = new Frame();

            animationFrame.image = ResolveImage(file, container, self);
            animationFrame.delay = self.HasChild("delay") ? self["delay"].ValueOrDefault<int>(0) : 0;
            animationFrame.origin = self.HasChild("origin") ? ((WZPointProperty)self["origin"]).Value : new Point(0, 0);
            animationFrame.Position = self.HasChild("z") ? self["z"].ValueOrDefault<string>("") : null;
            animationFrame.MapOffset = self.HasChild("map") ? self["map"].Select(c => new Tuple<string, Point>(c.Name, ((WZPointProperty)c).Value)).ToDictionary(b => b.Item1, b => b.Item2) : null;

            return animationFrame;
        }

        static Bitmap ResolveImage(WZObject file, WZObject container, WZObject self)
        {
            WZObject image = self;

            bool hasChanged = false;
            List<string> attemptedPaths = new List<string>();
            do
            {
                if (attemptedPaths.Contains(image.Path)) break;
                attemptedPaths.Add(image.Path);

                hasChanged = false;
                if (image.HasChild("_inlink"))
                {
                    try
                    {
                        image = container.ResolvePath("../../" + image["_inlink"].ValueOrDefault<string>(""));
                        hasChanged = true;
                    } catch (Exception ex)
                    {
                        try
                        {
                            image = container.ResolvePath("../" + image["_inlink"].ValueOrDefault<string>(""));
                            hasChanged = true;
                        }
                        catch (Exception innerEx)
                        {
                            image = container.ResolvePath(image["_inlink"].ValueOrDefault<string>(""));
                            hasChanged = true;
                        }
                    }
                }
                if (image.HasChild("_outlink"))
                {
                    string outlink = image["_outlink"].ValueOrDefault<string>("");
                    if (outlink.StartsWith("Skill/"))
                    {
                        image = file.ResolvePath(outlink.Substring(6));
                        hasChanged = true;
                    }
                    else if (outlink.StartsWith("Mob/"))
                    {
                        image = file.ResolvePath(outlink.Substring(4));
                        hasChanged = true;
                    }
                    else
                    {
                        // This does not support linking out to a different file, might need to be fixed at some point. :(
                        image = file.ResolvePath(outlink.Substring(outlink.IndexOf('/') + 1));
                        hasChanged = true;
                    }
                }
                if (image is WZUOLProperty)
                {
                    string name = image.ValueOrDefault<string>("");
                    image = self.ResolvePath("../" + name);
                    hasChanged = true;
                }
            } while (hasChanged);

            try
            {
                return ((WZCanvasProperty)image).ValueOrDefault<Bitmap>(null);
            } catch (Exception ex)
            {
                // *shrug*
                return null;
            }
        }
    }
}