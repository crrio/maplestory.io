using System;
using System.Collections.Generic;
using System.Text;
using PKG1;

namespace maplestory.io.Data.Mobs
{
    public class MobInfo
    {
        public int Id;
        public string Name;

        public MobInfo(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public static MobInfo Parse(WZProperty stringWz)
            => stringWz == null ? null : new MobInfo(
                int.Parse(stringWz.NameWithoutExtension),
                stringWz.ResolveForOrNull<string>("name")
            );

        public static MobInfo GetFromId(WZProperty anyWz, int mobId)
            => Parse(anyWz.ResolveOutlink($"String/Mob/{mobId}"));
    }
}
