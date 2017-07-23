using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;

namespace WZData.ItemMetaInfo
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
            if (!info.Children.Keys.Any(c => mustContainOne.Contains(c)))
                return null;

            ChairInfo results = new ChairInfo();

            results.recoveryHP = info.ResolveFor<int>("recoveryHP");
            results.recoveryMP = info.ResolveFor<int>("recoveryMP");
            results.reqLevel = info.ResolveFor<int>("reqLevel");

            return results;
        }
    }
}
