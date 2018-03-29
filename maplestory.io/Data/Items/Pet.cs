using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using PKG1;
using maplestory.io.Data.Images;

namespace maplestory.io.Data.Items
{
    public class Pet : MapleItem
    {
        public Dictionary<string, IEnumerable<FrameBook>> frameBooks;
        public Pet(int id) : base(id) { }
        public static Pet Parse(WZProperty stringWz)
        {
            int id;

            if (!int.TryParse(stringWz.NameWithoutExtension, out id)) return null;

            Pet p = new Pet(id);
            WZProperty petEntry = stringWz.ResolveOutlink($"Item/Pet/{id}");

            p.frameBooks = petEntry.Children.Where(c => c.NameWithoutExtension != "info").ToDictionary(c => c.NameWithoutExtension, c => FrameBook.Parse(c));

            p.Description = ItemDescription.Parse(stringWz, id);

            p.MetaInfo = ItemInfo.Parse(petEntry);

            return p ?? null;
        }
    }
}
