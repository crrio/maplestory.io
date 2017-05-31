using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.Text;

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

        public static MapLife Parse(WZObject data)
        {
            MapLife result = new MapLife();

            result.X = data.HasChild("x") ? data["x"].ValueOrDefault<int>(0) : -1; // x
            result.Y = data.HasChild("y") ? data["y"].ValueOrDefault<int>(0) : -1; // y
            result.WalkAreaX1 = data.HasChild("rx0") ? data["rx0"].ValueOrDefault<int>(0) : -1; // rx0
            result.WalkAreaX2 = data.HasChild("rx1") ? data["rx1"].ValueOrDefault<int>(0) : -1; // rx1
            if (data.HasChild("id"))
            {
                WZObject idObject = data["id"];
                switch (idObject.Type)
                {
                    case WZObjectType.Int32:
                        result.Id = idObject.ValueOrDefault<int>(0);
                        break;
                    case WZObjectType.String:
                        result.Id = int.Parse(idObject.ValueOrDefault<string>(""));
                        break;
                }
            }
            result.Foothold = data.HasChild("fh") ? data["fh"].ValueOrDefault<int>(0) : -1; // fh
            result.Hidden = data["hide"].ValueOrDefault<int>(0) == 1; // hide
            result.Type = data["type"].ValueOrDefault<string>("").ToLower() == "n" ? LifeType.NPC : LifeType.Monster; // type

            return result;
        }
    }

    public enum LifeType
    {
        NPC,
        Monster
    }
}
