using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.Text;

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

        public static Portal Parse(WZObject portalData)
            => new Portal()
            {
                PortalName = portalData.HasChild("pn") ? portalData["pn"].ValueOrDefault<string>(null) : null,
                ToMap = portalData.HasChild("tm") ? portalData["tm"].ValueOrDefault<int>(-1) : -1,
                ToName = portalData.HasChild("tn") ? portalData["tn"].ValueOrDefault<string>(null) : null,
                Type = portalData.HasChild("pt") ? (PortalType)portalData["pt"].ValueOrDefault<int>(0) : PortalType.Spawn,
                x = portalData.HasChild("x") ? portalData["x"].ValueOrDefault<int>(-99999999) : -99999999,
                y = portalData.HasChild("y") ? portalData["y"].ValueOrDefault<int>(-99999999) : -99999999
            };
    }
}
