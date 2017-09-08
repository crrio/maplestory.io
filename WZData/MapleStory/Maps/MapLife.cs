using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Newtonsoft.Json;
using PKG1;
using SixLabors.Primitives;
using WZData.MapleStory.Images;
using System.Collections.Concurrent;

namespace WZData.MapleStory.Maps
{
    public class MapLife : IPositionedFrameContainer
    {
        public int X;
        public int Y;
        public int WalkAreaX1;
        public int WalkAreaX2;
        public int Id;
        public int? FootholdId;
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
        public string Name;
        [JsonIgnore]
        public Frame Canvas { get; set; }
        public Vector3 Position { get => new Vector3(X, Y, 1); }
        public RectangleF Bounds {
            get => Canvas == null ? new RectangleF(X, Y, 1, 1) : new RectangleF(new Point(X - Canvas.OriginOrZero.X,Y - Canvas.OriginOrZero.Y), new Size(Canvas?.Image?.Width ?? 1, Canvas?.Image?.Height ?? 1));
        }
        public bool Flip { get; }
        public static MapLife Parse(WZProperty data)
            => Parse(data, new ConcurrentDictionary<int, Tuple<string, Frame>>());

        public static MapLife Parse(WZProperty data, ConcurrentDictionary<int, Tuple<string, Frame>> lifeLookup)
        {
            MapLife result = new MapLife();

            result.X = data.ResolveFor<int>("x") ?? int.MinValue; // x
            result.Y = data.ResolveFor<int>("y") ?? int.MinValue; // y
            result.WalkAreaX1 = data.ResolveFor<int>("rx0") ?? int.MinValue; // rx0
            result.WalkAreaX2 = data.ResolveFor<int>("rx1") ?? int.MinValue; // rx1
            result.Id = data.ResolveFor<int>("id") ?? -1;
            result.FootholdId = data.ResolveFor<int>("fh"); // fh
            result.Hidden = data.ResolveFor<bool>("hide") ?? false; // hide
            result.Type = data.ResolveForOrNull<string>("type").ToLower() == "n" ? LifeType.NPC : LifeType.Monster; // type

            if (lifeLookup.ContainsKey(result.Id))
            {
                result.Name = lifeLookup[result.Id].Item1;
                result.Canvas = lifeLookup[result.Id].Item2;
            }
            else
            {
                if (result.Type == LifeType.NPC)
                {
                    result.Name = NPC.NPC.GetName(data, result.Id);
                    result.Canvas = NPC.NPC.GetFirstFrame(data, result.Id);
                }
                else if (result.Type == LifeType.Monster)
                {
                    result.Name = Mobs.Mob.GetName(data, result.Id);
                    result.Canvas = Mobs.Mob.GetFirstFrame(data, result.Id);
                }
                lifeLookup.AddOrUpdate(result.Id, new Tuple<string, Frame>(result.Name, result.Canvas), (a, b) => b);
            }

            return result;
        }

        public void UpdateWithFH(Foothold fh)
        {
            Foothold = fh;
            Y = Foothold.YAtX(X);
        }

        internal static MapLife Parse(WZProperty c, Dictionary<int, Foothold> footholds, ConcurrentDictionary<int, Tuple<string, Frame>> lifeLookup)
        {
            MapLife result = Parse(c, lifeLookup);
            if (result.FootholdId.HasValue && footholds.ContainsKey(result.FootholdId.Value))
                result.UpdateWithFH(footholds[result.FootholdId.Value]);
            return result;
        }
    }

    public enum LifeType
    {
        NPC,
        Monster
    }
}