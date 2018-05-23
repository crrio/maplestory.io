using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PKG1;

namespace maplestory.io.Data
{
    public class ZMap
    {
        public IEnumerable<string> Ordering;

        public static ZMap Parse(WZProperty BaseWz)
            => new ZMap() {
                Ordering = BaseWz.Resolve("zmap").Children
                    .Select(c => c.NameWithoutExtension)
                    .ToArray()
                    .Reverse()
            };
    }
}
