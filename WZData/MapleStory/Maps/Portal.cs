using System;
using System.Collections.Generic;
using System.Text;
using PKG1;

namespace WZData.MapleStory.Maps
{
    public class Portal
    {
        public enum PortalType
        {
            Spawn = 0,
            HiddenTeleport = 1,
            Portal = 2,
            ForceTeleport = 3,
            HintTeleport = 10
        }

        public string PortalName, ToName;
        public PortalType Type;
        public int ToMap, x, y;
        public string LinkToMap => ToMap == 999999999 ? null : $"/map/{ToMap}";

        public static Portal Parse(WZProperty portalData)
            => new Portal()
            {
                PortalName = portalData.ResolveForOrNull<string>("pn"),
                ToMap = portalData.ResolveFor<int>("tm") ?? int.MinValue,
                ToName = portalData.ResolveForOrNull<string>("tn"),
                Type = (PortalType)(portalData.ResolveFor<int>("pt") ?? 0),
                x = portalData.ResolveFor<int>("x") ?? int.MinValue,
                y = portalData.ResolveFor<int>("y") ?? int.MinValue
            };
    }
}
