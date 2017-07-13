using System;
using reWZ.WZProperties;
using ImageSharp;
using System.Collections.Generic;
using System.Linq;
using ImageSharp;
using System.Numerics;

namespace WZData
{
    public class Frame : IFrame
    {
        public Image<Rgba32> Image { get; set; }
        public int delay;
        public Vector2? Origin { get; set; }
        public Dictionary<string, Vector2> MapOffset { get; set; }
        public string Position { get; set; }

        internal static Frame Parse(WZObject file, WZObject container, WZObject self)
        {
            Frame animationFrame = new Frame();

            animationFrame.Image = ResolveImage(file, container, self);
            animationFrame.delay = self.HasChild("delay") ? self["delay"].ValueOrDefault<int>(0) : 0;
            animationFrame.Origin = self.HasChild("origin") ? ((WZVector2Property)self["origin"]).Value : new Vector2(0, 0);
            if (self.Parent.HasChild("z")) {
                WZObject zContainer = self.Parent["z"];
                animationFrame.Position = zContainer.Type == WZObjectType.String ? zContainer.ValueOrDefault<string>("") : zContainer.ValueOrDefault<int>(0).ToString();
            } else if (self.HasChild("z"))
                animationFrame.Position = self["z"].Type == WZObjectType.String ? self["z"].ValueOrDefault<string>("") : self["z"].ValueOrDefault<int>(0).ToString();
            animationFrame.MapOffset = self.HasChild("map") ? self["map"].Where(c => c.Type == WZObjectType.Vector2).Select(c => new Tuple<string, Vector2>(c.Name, ((WZVector2Property)c).Value)).ToDictionary(b => b.Item1, b => b.Item2) : null;

            return animationFrame;
        }

        static Image<Rgba32> ResolveImage(WZObject file, WZObject container, WZObject self)
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
                        // This does not support linking out to a different file, might need to be fixed at some Vector2. :(
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
                return ((WZCanvasProperty)image).ImageOrDefault();
            } catch (Exception ex)
            {
                // *shrug*
                return null;
            }
        }
        public override string ToString()
         => $"IFrame ({Position})";
    }

    public interface IFrame
    {
        Image<Rgba32> Image { get; }
        Vector2? Origin { get; }
        string Position { get; }
        Dictionary<string, Vector2> MapOffset { get; }
    }
}