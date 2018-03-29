using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;

namespace maplestory.io.Data.Items
{
    public class ChairInfo
    {
        readonly static string[] mustContainOne = new []{
            "recoveryHP",
            "recoveryMP",
            "reqLevel"
        };
        public int? recoveryHP;
        public int? recoveryMP;
        public int? reqLevel;

        public static ChairInfo Parse(WZProperty info)
        {
            if (!info.Children.Any(c => mustContainOne.Contains(c.NameWithoutExtension)))
                return null;

            ChairInfo results = new ChairInfo();

            results.recoveryHP = info.ResolveFor<int>("recoveryHP");
            results.recoveryMP = info.ResolveFor<int>("recoveryMP");
            results.reqLevel = info.ResolveFor<int>("reqLevel");

            return results;
        }
    }
}
