using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Newtonsoft.Json;
using PKG1;
using SixLabors.Primitives;
using maplestory.io.Data.Images;
using System.Linq;

namespace maplestory.io.Data.Maps
{
    public class Portal : IPositionedFrameContainer
    {
        public string PortalName, ToName;
        public PortalType Type;
        public int ToMap, x, y;
        public string LinkToMap => ToMap == 999999999 ? null : $"/map/{ToMap}";
        public MapName ToMapName;
        private PackageCollection collection;
        public string portalImage;
        public bool? onlyOnce;
        [JsonIgnore]
        public Map LinkedMap {
            get => ToMap == 999999999 ? null : Map.Parse(ToMap, null, collection);
        }
        public bool IsStarForcePortal;
        //    get => LinkedMap?.MinimumStarForce > 0;
        //}
        public bool UnknownExit { get => ToMap == 999999999; }

        [JsonIgnore]
        public Frame Canvas { get; set; }

        [JsonIgnore]
        public Vector3 Position {
            get => new Vector3(x, y, 0);
        }

        public RectangleF Bounds {
            get => Canvas == null ? new RectangleF(x, y, 1, 1) : new RectangleF(new Point(x - Canvas.OriginOrZero.X, y - Canvas.OriginOrZero.Y), new Size(Canvas.Image.Width, Canvas.Image.Height));
        }
        [JsonIgnore]
        public bool Flip => false;

        public static Portal Parse(WZProperty portalData)
        {
            Portal portal = new Portal()
            {
                collection = portalData.FileContainer.Collection,
                PortalName = portalData.ResolveForOrNull<string>("pn"),
                ToMap = portalData.ResolveFor<int>("tm") ?? int.MinValue,
                ToName = portalData.ResolveForOrNull<string>("tn"),
                Type = (PortalType)(portalData.ResolveFor<int>("pt") ?? 0),
                x = portalData.ResolveFor<int>("x") ?? int.MinValue,
                y = portalData.ResolveFor<int>("y") ?? int.MinValue,
                ToMapName = MapName.GetMapNameLookup(portalData)[portalData.ResolveFor<int>("tm") ?? -1].FirstOrDefault(),
                portalImage = portalData.ResolveForOrNull<string>("image"),
                onlyOnce = portalData.ResolveFor<bool>("onlyOnce"),
                Canvas = Frame.Parse(portalData.ResolveOutlink($"Map/MapHelper/portal/game/pv/{portalData.ResolveForOrNull<string>("image") ?? "default"}/0"))
            };

            if (!portal.UnknownExit)
                portal.IsStarForcePortal = (portalData.ResolveOutlinkFor<int>($"Map/Map/Map{portal.ToMap.ToString("D8")[0]}/{portal.ToMap.ToString("D8")}.img/info/barrier") ?? 0) > 0;

            return portal;
        }
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
