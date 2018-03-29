using maplestory.io.Entities;
using maplestory.io.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PKG1;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace maplestory.io.Models
{
    public class MSPackageCollection : PackageCollection
    {
        public static ILogger<MSPackageCollection> Logger;
        static ConcurrentDictionary<string, EventWaitHandle> wzLoading = new ConcurrentDictionary<string, EventWaitHandle>();
        public static Dictionary<int, string> JobNameLookup = new Dictionary<int, string>()
        {
            { 0, "Beginner" },
            { 1, "Warrior" },
            { 2, "Magician"},
            { 4, "Bowman" },
            { 8, "Thief" },
            { 16, "Pirate" }
        };
        MapleVersion MapleVersion;
        public Dictionary<int, string> categoryFolders;
        public IDictionary<int, Tuple<string[], byte?, bool>> EquipMeta;
        public IDictionary<int, int[]> ItemDrops;
        public MSPackageCollection() { }
        public MSPackageCollection(string baseFilePath, ushort? versionId = null, Region region = Region.GMS) : base(baseFilePath, versionId, region) { }
        public MSPackageCollection(ApplicationDbContext db, MapleVersion versionInfo, ushort? versionId = null, Region region = Region.GMS) 
            : base(File.Exists(Path.Combine(versionInfo.Location, $"{versionInfo.BaseFile}.rebuilt.wz")) ? Path.Combine(versionInfo.Location, $"{versionInfo.BaseFile}.rebuilt.wz") : Path.Combine(versionInfo.Location, $"{versionInfo.BaseFile}.wz"), versionId, region)
        {
            this.MapleVersion = versionInfo;

            EventWaitHandle waitForWZ = new EventWaitHandle(false, EventResetMode.ManualReset);
            bool isInitial = wzLoading.TryAdd(versionInfo.Location, waitForWZ);

            if (!isInitial)
                wzLoading[versionInfo.Location].WaitOne();

            List<Task> loading = new List<Task>();

            string characterFoldersPath = Path.Combine(versionInfo.Location, "characterFolders.json");
            if (File.Exists(characterFoldersPath))
                categoryFolders = new Dictionary<int, string>(JsonConvert.DeserializeObject<Dictionary<int, string>>(File.ReadAllText(characterFoldersPath)));
            else
                loading.Add(CacheCharacterFolders(db, characterFoldersPath));

            string equipMetaPath = Path.Combine(versionInfo.Location, "equipMeta.json");
            if (File.Exists(equipMetaPath))
                EquipMeta = new Dictionary<int, Tuple<string[], byte?, bool>>(JsonConvert.DeserializeObject<Dictionary<int, Tuple<string[], byte?, bool>>>(File.ReadAllText(equipMetaPath)));
            else
                loading.Add(CacheEquipMeta(db, equipMetaPath));

            string dropPath = Path.Combine(versionInfo.Location, "itemDrops.json");
            if (File.Exists(dropPath))
                ItemDrops = JsonConvert.DeserializeObject<Dictionary<int, int[]>>(File.ReadAllText(dropPath));
            else loading.Add(CacheDropLookup(db, dropPath));

            Task.WaitAll(loading.ToArray());
            if (isInitial)
            {
                Logger.LogInformation("{0} has been loaded", versionInfo.Location);
                waitForWZ.Set();
            }
        }

        Task CacheCharacterFolders(ApplicationDbContext db, string characterFoldersPath)
        {
            return Task.Run(() =>
            {
                Logger.LogInformation("Caching character folders for {0}", MapleVersion.Location);
                categoryFolders = new Dictionary<int, string>();
                DbConnection con = db.Database.GetDbConnection();
                if (con.State == System.Data.ConnectionState.Closed) con.Open();
                MySqlCommand com = new MySqlCommand(@"SELECT CONVERT(`categoryId`, UNSIGNED), ANY_VALUE(folder) FROM (SELECT 
    *,
    @Num:= CONVERT(`ImgName`, UNSIGNED) as Num,
    floor(@Num / 100) categoryId,
    substr(`Path`, 10),
    substr(`Path`, 11, locate('\\', `PATH`, 11) - 11) folder
FROM 
	`maplestory.io`.`VersionPathHashes`
WHERE `MapleVersionId` = " + MapleVersion.Id + @"
AND `PackageName` = 'Character'
) a
WHERE `categoryId` IS NOT NULL
GROUP BY `categoryId`
ORDER BY ANY_VALUE(`folder`)", (MySqlConnection)con);
                using (MySqlDataReader reader = com.ExecuteReader())
                    while (reader.Read())
                        categoryFolders.Add(Convert.ToInt32(reader[0]), (string)reader[1]);
                File.WriteAllText(characterFoldersPath, JsonConvert.SerializeObject(categoryFolders));
            });
        }

        Task CacheEquipMeta(ApplicationDbContext db, string equipMetaPath)
        {
            return Task.Run(() =>
            {
                Logger.LogInformation("Caching equip meta for {0}", MapleVersion.Location);
                ConcurrentDictionary<int, Tuple<string[], byte?, bool>> regionData = new ConcurrentDictionary<int, Tuple<string[], byte?, bool>>();

                while (!Parallel.ForEach(
                    Resolve("Character").Children
                        .SelectMany(c => c.Resolve().Children),
                    c =>
                    {
                        if (!int.TryParse(c.NameWithoutExtension, out int itemId)) return;
                        int reqJob = c.ResolveFor<int>("info/reqJob") ?? 0;
                        regionData.TryAdd(
                            itemId,
                            new Tuple<string[], byte?, bool>(
                                JobNameLookup.Where(b => (b.Key & reqJob) == b.Key && (b.Key != 0 || reqJob == 0)).Select(b => b.Value).ToArray(),
                                c.ResolveFor<byte>("info/reqLevel"),
                                c.ResolveFor<bool>("info/cash") ?? false
                            )
                        );
                    }
                ).IsCompleted) Thread.Sleep(1);

                File.WriteAllText(equipMetaPath, JsonConvert.SerializeObject(regionData));
                EquipMeta = regionData;
            });
        }

        Task CacheDropLookup(ApplicationDbContext db, string dropPath)
        {
            return Task.Run(() =>
            {
                Logger.LogInformation("Caching drops for {0}", MapleVersion.Location);
                IDictionary<int, int[]> dropBy = Resolve("String/MonsterBook")?
                .Children
                .AsParallel()
                .SelectMany(c =>
                    c.Resolve("reward").Children
                    .Select(b =>
                        new Tuple<int, int?>(int.Parse(c.NameWithoutExtension), b.ResolveFor<int>())
                    ).Where(b => b.Item2 != null)
                )
                .ToLookup(c => c.Item2.Value, c => c.Item1).ToDictionary(c => c.Key, c => c.ToArray());

                File.WriteAllText(dropPath, JsonConvert.SerializeObject(dropBy));
                ItemDrops = dropBy;
            });
       }

        public class LookupSerializer : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                var result = objectType.GetInterfaces().Any(a => a.IsGenericType
                    && a.GetGenericTypeDefinition() == typeof(ILookup<,>));
                return result;
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var obj = new JObject();
                var enumerable = (IEnumerable)value;

                foreach (object kvp in enumerable)
                {
                    // TODO: caching
                    var keyProp = kvp.GetType().GetProperty("Key");
                    var keyValue = keyProp.GetValue(kvp, null);

                    obj.Add(keyValue.ToString(), JArray.FromObject((IEnumerable)kvp));
                }

                obj.WriteTo(writer);
            }
        }
    }
}
