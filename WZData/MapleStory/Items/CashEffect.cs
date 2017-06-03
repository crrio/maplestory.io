using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using reWZ.WZProperties;
using System.Reflection;

namespace WZData.MapleStory.Items
{
    public class CashEffect
    {
        public static List<string> unknownEffects = new List<string>();
        public Dictionary<string, IEnumerable<FrameBook>> framebooks;

        public bool isFollow;
        public static CashEffect Parse(WZDirectory itemWz, WZObject cashItem, WZObject effects)
        {
            CashEffect effect = new CashEffect();

            bool isOnlyDefault = false;

            if (effects.HasChild("follow"))
                effect.isFollow = effects["follow"].ValueOrDefault<int>(0) == 1;

            foreach (WZObject obj in effects.Where(c => c.Name != "follow"))
            {
                int frameTest = 0;
                if (isOnlyDefault = (obj.Type == WZObjectType.Canvas || int.TryParse(obj.Name, out frameTest))) break;

                if (obj.ChildCount == 0) continue;
                effect.framebooks.Add(obj.Name, FrameBook.Parse(itemWz, cashItem, obj));
            }

            if (isOnlyDefault)
                effect.framebooks.Add("default", FrameBook.Parse(itemWz, cashItem, effects));

            return effect;
        }
    }
}
