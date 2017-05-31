using reWZ.WZProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WZData.MapleStory
{
    public class Tips
    {
        public int? All;
        public string TipGroupName;
        public byte? LevelMin;
        public byte? LevelMax;
        public int? Job;
        public int? Interval;
        public IEnumerable<string> Messages;
        public WorldType World;

        public static Tips Parse(WZObject tipMessages, WZObject tipInfo, WorldType worldType, string[] AllMessages)
        {
            Tips result = new Tips();

            result.All = tipInfo.HasChild("all") ? (int?)tipInfo["all"].ValueOrDefault<int>(0) : null;
            result.TipGroupName = tipInfo.HasChild("tip") ? tipInfo["tip"].ValueOrDefault<string>("") : "all";
            result.LevelMin = tipInfo.HasChild("levelMin") ? (byte?)tipInfo["levelMin"].ValueOrDefault<int>(0) : null;
            result.LevelMax = tipInfo.HasChild("levelMax") ? (byte?)tipInfo["levelMax"].ValueOrDefault<int>(0) : null;
            result.Job = tipInfo.HasChild("job") ? (int?)tipInfo["job"].ValueOrDefault<int>(0) : null;
            result.Interval = tipInfo.HasChild("interval") ? (int?)tipInfo["interval"].ValueOrDefault<int>(0) : null;
            result.Messages = tipMessages.Select(c => c.ValueOrDefault<string>("")).Concat(AllMessages).Distinct();
            result.World = worldType;

            return result;
        }

        public static IEnumerable<Tips> GetTips(WZObject etcWz)
        {
            return etcWz.ResolvePath("Tips.img")
                .Select(c => new Tuple<WorldType, WZObject>((WorldType)Enum.Parse(typeof(WorldType), c.Name, true), c))
                .Select(c => {
                    string[] allMessages = c.Item2["all"].Select(b => b.ValueOrDefault("")).ToArray();
                    return c.Item2["info"].Select(b =>
                    {
                        WZObject messageContainer = c.Item2[b.HasChild("tip") ? b["tip"].ValueOrDefault<string>("") : "all"];
                        return Parse(messageContainer, b, c.Item1, allMessages);
                    });
                })
                .SelectMany(c => c);
        }
    }

    public enum WorldType
    {
        Normal,
        Reboot
    }
}
