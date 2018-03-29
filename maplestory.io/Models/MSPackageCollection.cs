using maplestory.io.Entities;
using maplestory.io.Entities.Models;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using PKG1;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace maplestory.io.Models
{
    public class MSPackageCollection : PackageCollection
    {
        static ConcurrentDictionary<string, EventWaitHandle> wzLoading = new ConcurrentDictionary<string, EventWaitHandle>();
        MapleVersion MapleVersion;
        public Dictionary<int, string> categoryFolders;
        public MSPackageCollection() { }
        public MSPackageCollection(string baseFilePath, ushort? versionId = null, Region region = Region.GMS) : base(baseFilePath, versionId, region) { }
        public MSPackageCollection(ApplicationDbContext db, MapleVersion versionInfo, ushort? versionId = null, Region region = Region.GMS) 
            : base(File.Exists(Path.Combine(versionInfo.Location, $"{versionInfo.BaseFile}.rebuilt.wz")) ? Path.Combine(versionInfo.Location, $"{versionInfo.BaseFile}.rebuilt.wz") : Path.Combine(versionInfo.Location, $"{versionInfo.BaseFile}.wz"), versionId, region)
        {
            this.MapleVersion = versionInfo;
            List<Task> loading = new List<Task>();
            string characterFoldersPath = Path.Combine(versionInfo.Location, "characterFolders.json");
            if (File.Exists(characterFoldersPath))
                categoryFolders = new Dictionary<int, string>(JsonConvert.DeserializeObject<Dictionary<int, string>>(File.ReadAllText(characterFoldersPath)));
            else
                loading.Add(CacheCharacterFolders(db, characterFoldersPath));

            foreach (Task t in loading) t.Start();
            Task.WaitAll(loading.ToArray());
        }

        Task CacheCharacterFolders(ApplicationDbContext db, string characterFoldersPath)
        {
            return new Task(() =>
            {
                EventWaitHandle waitForCharacterFolders = new EventWaitHandle(false, EventResetMode.ManualReset);
                if (wzLoading.TryAdd(characterFoldersPath, waitForCharacterFolders))
                {
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
WHERE `MapleVersionId` = "+MapleVersion.Id+@"
AND `PackageName` = 'Character'
) a
WHERE `categoryId` IS NOT NULL
GROUP BY `categoryId`
ORDER BY ANY_VALUE(`folder`)", (MySqlConnection)con);
                    using (MySqlDataReader reader = com.ExecuteReader())
                        while (reader.Read())
                            categoryFolders.Add(Convert.ToInt32(reader[0]), (string)reader[1]);
                    File.WriteAllText(characterFoldersPath, JsonConvert.SerializeObject(categoryFolders));
                    waitForCharacterFolders.Set();
                }
                else
                {
                    wzLoading[characterFoldersPath].WaitOne();
                    categoryFolders = new Dictionary<int, string>(JsonConvert.DeserializeObject<IEnumerable<KeyValuePair<int, string>>>(File.ReadAllText(characterFoldersPath)));
                }
            });
        }
    }
}
