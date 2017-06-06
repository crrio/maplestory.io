using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WZData.MapleStory
{
    public class SMap
    {
        public IEnumerable<Tuple<string, string>> Ordering;

        public static SMap Parse(WZObject BaseWz)
            => new SMap() { Ordering = BaseWz["smap.img"]
                .Where(c => c is WZStringProperty)
                .Select(c => new Tuple<string, string>(c.Name, ((WZStringProperty) c).Value)).ToArray() };
    }
}
