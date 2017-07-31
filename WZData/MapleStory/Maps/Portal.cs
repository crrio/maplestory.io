using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Newtonsoft.Json;
using PKG1;
using SixLabors.Primitives;
using WZData.MapleStory.Images;

namespace WZData.MapleStory.Maps
{
    public class Portal : IPositionedFrameContainer
    {
        public string PortalName, ToName;
        public PortalType Type;
        public int ToMap, x, y;
        public string LinkToMap => ToMap == 999999999 ? null : $"/map/{ToMap}";
        private PackageCollection collection;
        public string portalImage;
        public bool? onlyOnce;

        public Map LinkedMap {
            get => Map.Parse(ToMap, null, collection);
        }
        public bool IsStarForcePortal {
            get => LinkedMap.MinimumStarForce > 0;
        }

        [JsonIgnore]
        public Frame Canvas { get; set; }

        [JsonIgnore]
        public Vector3 Position {
            get => new Vector3(x, y, 0);
        }

        public RectangleF Bounds {
            get => Canvas == null ? new RectangleF(x, y, 1, 1) : new RectangleF(new Point(x - Canvas.OriginOrZero.X,y - Canvas.OriginOrZero.Y), new Size(Canvas.Image.Width, Canvas.Image.Height));
        }
        [JsonIgnore]
        public bool Flip => false;

        public static Portal Parse(WZProperty portalData)
            => new Portal()
            {
                collection = portalData.FileContainer.Collection,
                PortalName = portalData.ResolveForOrNull<string>("pn"),
                ToMap = portalData.ResolveFor<int>("tm") ?? int.MinValue,
                ToName = portalData.ResolveForOrNull<string>("tn"),
                Type = (PortalType)(portalData.ResolveFor<int>("pt") ?? 0),
                x = portalData.ResolveFor<int>("x") ?? int.MinValue,
                y = portalData.ResolveFor<int>("y") ?? int.MinValue,
                portalImage = portalData.ResolveForOrNull<string>("image"),
                onlyOnce = portalData.ResolveFor<bool>("onlyOnce"),
                Canvas = Frame.Parse(portalData.ResolveOutlink($"Map/MapHelper/portal/game/pv/{portalData.ResolveForOrNull<string>("image") ?? "default"}/0"))
            };
    }

    public enum PortalType
    {
        Spawn = 0,
        HiddenTeleport = 1,
        Portal = 2,
        ForceTeleport = 3,
        HintTeleport = 10
    }
}
