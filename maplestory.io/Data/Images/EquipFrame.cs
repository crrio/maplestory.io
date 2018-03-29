using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using PKG1;

namespace maplestory.io.Data.Images
{
    public class EquipFrame
    {
        public static List<string> unknownEffects = new List<string>();
        public Dictionary<string, Frame> Effects;

        internal static EquipFrame Parse(WZProperty frame)
        {
            EquipFrame item = new EquipFrame();

            item.Effects = frame.Children.Where(c => c.Type == PropertyType.Canvas || c.Type == PropertyType.UOL)
                .ToDictionary(c => c.NameWithoutExtension, c => Frame.Parse(c));

            return item;

        }
    }
}
