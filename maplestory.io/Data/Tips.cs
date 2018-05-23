using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PKG1;

namespace maplestory.io.Data
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

        public static Tips Parse(WZProperty tipMessages, WZProperty tipInfo, WorldType worldType, string[] AllMessages)
        {
            Tips result = new Tips();

            result.All = tipInfo.ResolveFor<int>("all");
            result.TipGroupName = tipInfo.ResolveForOrNull<string>("tip") ?? "all";
            result.LevelMin = tipInfo.ResolveFor<byte>("levelMin");
            result.LevelMax = tipInfo.ResolveFor<byte>("levelMax");
            result.Job = tipInfo.ResolveFor<int>("job");
            result.Interval = tipInfo.ResolveFor<int>("interval");
            result.Messages = tipMessages.Children.Select(c => ((IWZPropertyVal)c).GetValue().ToString()).Concat(AllMessages).Distinct();
            result.World = worldType;

            return result;
        }

        public static IEnumerable<Tips> GetTips(WZProperty etcWz)
        {
            return etcWz.Resolve("Tips").Children
                .Select(c => new Tuple<WorldType, WZProperty>((WorldType)Enum.Parse(typeof(WorldType), c.NameWithoutExtension, true), c))
                .Select(c => {
                    string[] allMessages = c.Item2.Resolve("all").Children.Select(b => ((IWZPropertyVal)b).GetValue().ToString()).ToArray();
                    return c.Item2.Resolve("info").Children.Select(b =>
                    {
                        WZProperty messageContainer = c.Item2.Resolve(b.ResolveForOrNull<string>("tip") ?? "all");
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
