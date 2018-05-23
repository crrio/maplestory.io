using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using PKG1;
using maplestory.io.Data.Images;

namespace maplestory.io.Data.Items
{
    public class CashEffect
    {
        public static List<string> unknownEffects = new List<string>();
        public Dictionary<string, IEnumerable<FrameBook>> framebooks = new Dictionary<string, IEnumerable<FrameBook>>();

        public bool isFollow;

        internal static CashEffect Parse(WZProperty wZProperty)
        {
            CashEffect effect = new CashEffect();

            bool isOnlyDefault = false;

            effect.isFollow = wZProperty.ResolveFor<bool>("follow") ?? false;

            foreach (WZProperty obj in wZProperty.Children.Where(c => c.NameWithoutExtension != "follow" && c.NameWithoutExtension != "info"))
            {
                int frameTest = 0;
                if (isOnlyDefault = (obj.Type == PropertyType.Canvas || int.TryParse(obj.NameWithoutExtension, out frameTest))) break;

                if (obj.Children.Count() == 0) continue;
                effect.framebooks.Add(obj.NameWithoutExtension, FrameBook.Parse(obj));
            }

            if (isOnlyDefault)
                effect.framebooks.Add("default", FrameBook.Parse(wZProperty));

            return effect;
        }
    }
}
