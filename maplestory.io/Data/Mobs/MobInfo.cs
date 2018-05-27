using System;
using System.Collections.Generic;
using System.Text;
using maplestory.io.Models;
using PKG1;

namespace maplestory.io.Data.Mobs
{
    public class MobInfo
    {
        public int Id;
        public string Name;
        public string MobType;
        public int? Level;
        public bool? IsBoss;

        public MobInfo(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public MobInfo(int id, string name, string mobType, int level, bool isBoss) : this(id, name)
        {
            this.Id = id;
            this.Name = name;
            this.MobType = mobType;
            this.Level = level;
            this.IsBoss = isBoss;
        }

        public static MobInfo Parse(WZProperty stringWz)
        {
            if (stringWz == null)
                return null;
            else
            {
                int mobId = int.Parse(stringWz.NameWithoutExtension);
                if (stringWz.FileContainer.Collection is MSPackageCollection)
                {
                    MSPackageCollection collection = (MSPackageCollection)stringWz.FileContainer.Collection;
                    if (collection.MobMeta.ContainsKey(mobId))
                    {
                        Tuple<string, int, bool> mobMeta = collection.MobMeta[mobId];
                        return new MobInfo(
                            mobId,
                            stringWz.ResolveForOrNull<string>("name"),
                            mobMeta.Item1,
                            mobMeta.Item2,
                            mobMeta.Item3
                        );
                    }
                }
                return new MobInfo(
                    mobId,
                    stringWz.ResolveForOrNull<string>("name")
                );
            }
        }

        public static MobInfo GetFromId(WZProperty anyWz, int mobId)
            => Parse(anyWz.ResolveOutlink($"String/Mob/{mobId}"));
    }
}
