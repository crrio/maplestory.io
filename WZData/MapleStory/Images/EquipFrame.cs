using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using PKG1;

namespace WZData.MapleStory.Images
{
    public class EquipFrame
    {
        public static List<string> unknownEffects = new List<string>();
        public Dictionary<string, Frame> Effects;

        internal static EquipFrame Parse(WZProperty frame)
        {
            EquipFrame item = new EquipFrame();

            item.Effects = frame.Children.Where(c => c.Value.Type == PropertyType.Canvas || c.Value.Type == PropertyType.UOL)
                .ToDictionary(c => c.Key, c => Frame.Parse(c.Value));

            return item;

        }
    }
}
