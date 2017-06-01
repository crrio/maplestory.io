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

            item.Effects = frame.Where(c => c is WZCanvasProperty || c is WZUOLProperty)
                .Where(c => c.Name != "hairShade")
                .Select(c => c is WZUOLProperty ? ((WZUOLProperty)c).ResolveFully() : c)
                .Select(c => new Tuple<string, Frame>(c.Name, Frame.Parse(skills, skill, c)))
                .ToDictionary(c => c.Item1, c => c.Item2);

            return item;
        }
    }
}
