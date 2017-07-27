using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Newtonsoft.Json;
using PKG1;
using SixLabors.Primitives;
using WZData.MapleStory.Images;

namespace WZData.MapleStory.Maps
{
    public class MapLife : IPositionedFrameContainer
    {
        public int X;
        public int Y;
        public int WalkAreaX1;
        public int WalkAreaX2;
        public int Id;
        public int FootholdId;
        public Foothold Foothold;
        public bool Hidden;
        public LifeType Type;
        public string Link
        {
            get
            {
                if (this.Type == LifeType.NPC) return $"/npc/{Id}";
                return $"/mob/{Id}";
            }
        }
        [JsonIgnore]
        public Frame Canvas { get; set; }
        public Vector3 Position { get => new Vector3(X, Y, 1); }
        public RectangleF Bounds {
            get => new RectangleF(new Point(X - Canvas.OriginOrZero.X, Y - Canvas.OriginOrZero.Y), new Size(Canvas.Image.Width, Canvas.Image.Height));
        }
        public bool Flip { get; }
        public static MapLife Parse(WZProperty data)
            => Parse(data, new Dictionary<int, Frame>());

        public static MapLife Parse(WZProperty data, Dictionary<int, Frame> lifeLookup)
        {
            MapLife result = new MapLife();

            result.X = data.ResolveFor<int>("x") ?? int.MinValue; // x
            result.Y = data.ResolveFor<int>("y") ?? int.MinValue; // y
            result.WalkAreaX1 = data.ResolveFor<int>("rx0") ?? int.MinValue; // rx0
            result.WalkAreaX2 = data.ResolveFor<int>("rx1") ?? int.MinValue; // rx1
            result.Id = data.ResolveFor<int>("id") ?? -1;
            result.FootholdId = data.ResolveFor<int>("fh") ?? -1; // fh
            result.Hidden = data.ResolveFor<bool>("hide") ?? false; // hide
            result.Type = data.ResolveForOrNull<string>("type").ToLower() == "n" ? LifeType.NPC : LifeType.Monster; // type

            if (lifeLookup.ContainsKey(result.Id))
                result.Canvas = lifeLookup[result.Id];
            else {
                if (result.Type == LifeType.NPC)
                    result.Canvas = NPC.NPC.Parse(data.ResolveOutlink($"String/Npc/{result.Id}"))?.Framebooks?.Values.FirstOrDefault()?.FirstOrDefault()?.frames?.FirstOrDefault();
                else if (result.Type == LifeType.Monster)
                    result.Canvas = Mobs.Mob.Parse(data.ResolveOutlink($"String/Mob/{result.Id}"))?.Framebooks?.Values.FirstOrDefault()?.FirstOrDefault()?.frames?.FirstOrDefault();
                lifeLookup.Add(result.Id, result.Canvas);
            }

            return result;
        }

        public void UpdateWithFH(Foothold fh)
        {
            Foothold = fh;
            Y = Foothold.YAtX(X);
        }

        internal static MapLife Parse(WZProperty c, Dictionary<int, Foothold> footholds, Dictionary<int, Frame> lifeLookup)
        {
            MapLife result = Parse(c, lifeLookup);
            result.UpdateWithFH(footholds[result.FootholdId]);
            return result;
        }
    }

    public enum LifeType
    {
        NPC,
        Monster
    }
}