using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using reWZ.WZProperties;
using System.Reflection;

namespace WZData.MapleStory.Images
{
    public class EquipFrame
    {
        public static List<string> unknownEffects = new List<string>();
        public Dictionary<string, Frame> Effects;

        public static EquipFrame Parse(WZObject skills, WZObject skill, WZObject frame)
        {
            EquipFrame item = new EquipFrame();
            item.Effects = new Dictionary<string, Frame>();

            foreach (WZObject obj in frame)
            {
                Frame fr = Frame.Parse(skills, skill, obj);
                item.Effects.Add(obj.Name, fr);
            }

            return item;
        }
    }
}
