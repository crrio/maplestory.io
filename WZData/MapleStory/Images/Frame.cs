using System;
using reWZ.WZProperties;
using System.Drawing;
using System.Collections.Generic;

namespace WZData
{
    public class Frame
    {
        public Bitmap image;
        public int delay;
        public Point origin;
        public string Position;
        internal static Frame Parse(WZObject skills, WZObject skill, WZObject frame)
        {
            Frame animationFrame = new Frame();

            animationFrame.image = ResolveImage(skills, skill, frame);
            animationFrame.delay = frame.HasChild("delay") ? frame["delay"].ValueOrDefault<int>(0) : 0;
            animationFrame.origin = frame.HasChild("origin") ? ((WZPointProperty)frame["origin"]).Value : new Point(0, 0);
            animationFrame.Position = frame.HasChild("z") ? frame["z"].ValueOrDefault<string>("") : null;

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