using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WZData.MapleStory
{
    public class ZMap
    {
        public IEnumerable<string> Ordering;

        public static ZMap Parse(WZObject BaseWz)
            => new ZMap() { Ordering = BaseWz["zmap.img"].Select(c => c.Name).ToArray().Reverse() };
    }
}
