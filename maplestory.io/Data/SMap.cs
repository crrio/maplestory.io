using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PKG1;

namespace maplestory.io.Data
{
    public class SMap
    {
        public IEnumerable<Tuple<string, string>> Ordering;

        public static SMap Parse(WZProperty BaseWz)
            => new SMap() {
                Ordering = BaseWz.Resolve("smap").Children
                    .Where(c => c.Type == PropertyType.String)
                    .Select(c => new Tuple<string, string>(c.NameWithoutExtension, ((IWZPropertyVal)c).GetValue().ToString())).ToArray()
            };
    }
}
