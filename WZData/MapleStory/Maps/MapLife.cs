using System;
using System.Collections.Generic;
using System.Text;
using PKG1;

namespace WZData.MapleStory.Maps
{
    public class MapLife
    {
        public int X;
        public int Y;
        public int WalkAreaX1;
        public int WalkAreaX2;
        public int Id;
        public int Foothold;
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

        public static MapLife Parse(WZProperty data)
        {
            MapLife result = new MapLife();

            result.X = data.ResolveFor<int>("x") ?? int.MinValue; // x
            result.Y = data.ResolveFor<int>("y") ?? int.MinValue; // y
            result.WalkAreaX1 = data.ResolveFor<int>("rx0") ?? int.MinValue; // rx0
            result.WalkAreaX2 = data.ResolveFor<int>("rx1") ?? int.MinValue; // rx1
            result.Id = data.ResolveFor<int>("id") ?? -1;
            result.Foothold = data.ResolveFor<int>("fh") ?? -1; // fh
            result.Hidden = data.ResolveFor<bool>("hide") ?? false; // hide
            result.Type = data.ResolveForOrNull<string>("type").ToLower() == "n" ? LifeType.NPC : LifeType.Monster; // type

            return result;
        }
    }

    public enum LifeType
    {
        NPC,
        Monster
    }
}