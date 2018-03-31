using maplestory.io.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using maplestory.io.Entities.Models;

namespace maplestory.io.Controllers.API
{
    [Route("/api/{region}/{version}/diff")]
    public class DiffingController : APIController
    {
        const string DiffQuery = @"SELECT 
	`Path`,
    (SELECT count(*) FROM `VersionPathHashes` WHERE `ImgName` = a.`ImgName` && `MapleVersionId` in ({0}) && strcmp(`Path`, a.`Path`) = 0) > 0 as ExistedBefore,
    (SELECT `MapleVersionId` from `VersionPathHashes` b WHERE b.`Id` = a.`ResolvesTo`) HasntChangedSince
FROM 
	`VersionPathHashes` a
WHERE `ImgName` is not null 
AND (a.`ResolvesTo` is null OR (SELECT `MapleVersionId` from `VersionPathHashes` b WHERE b.`Id` = a.`ResolvesTo`) in ({0}))
AND `MapleVersionId` = @originalVersion;";

        public DiffingController(ApplicationDbContext dbCtx) => _ctx = dbCtx;
        private readonly ApplicationDbContext _ctx;

        [Route("{otherVersion}")]
        public IActionResult GetDiff(string otherVersion)
        {
            if (otherVersion.Equals(WZ.MapleVersion.MapleVersionId)) throw new InvalidOperationException("Cannot diff against self");

            List<long> versionIds = new List<long>();

            MapleVersion version = WZ.MapleVersion;
            while (version.MapleVersionId != otherVersion)
            {
                if (version.MapleVersionId.Equals(otherVersion)) break;
                version = _ctx.MapleVersions.FirstOrDefault(ver => ver.Id == version.BasedOffOf);
                versionIds.Add(version.Id);
            }

            string diffQueryIn = string.Format(DiffQuery, string.Join(',', versionIds));

            DbConnection con = _ctx.Database.GetDbConnection();
            if (!(con is MySqlConnection)) throw new InvalidOperationException("Cannot work with non-MySql database");
            if (con.State == System.Data.ConnectionState.Closed) con.Open();
            MySqlCommand command = new MySqlCommand(diffQueryIn, (MySqlConnection)con);

            command.Parameters.AddWithValue("@originalVersion", WZ.MapleVersion.Id);

            List<DiffEntry> diffEntries = new List<DiffEntry>();
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string path = (string)reader[0];
                    bool existedPrior = (long)reader[1] == 1;
                    string hasntChangedSince = reader.IsDBNull(2) ? null : (string)reader[2];
                    if (int.TryParse(hasntChangedSince, out int hasntChangedSinceVerNum))
                    {
                        diffEntries.Add(new DiffEntry()
                        {
                            ExistedPrior = existedPrior,
                            HasntChangedSince = hasntChangedSinceVerNum,
                            Path = path
                        });
                    }
                }
            }

            return Json(diffEntries);
        }

        public class DiffEntry
        {
            public string Path;
            public bool ExistedPrior;
            public int HasntChangedSince;
        }
    }
}
