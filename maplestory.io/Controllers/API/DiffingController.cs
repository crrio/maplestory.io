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
using Microsoft.Extensions.Primitives;

namespace maplestory.io.Controllers.API
{
    [Route("/api/{region}/{version}/diff")]
    public class DiffingController : APIController
    {
        const string DiffQuery =
            @"SELECT `Path`,
            (
              SELECT COUNT(*)
              FROM `VersionPathHashes`
              WHERE
                `ImgName` = a.`ImgName`
                AND `MapleVersionId` IN ({0})
                AND STRCMP(`Path`, a.`Path`) = 0
            ) AS ExistedBefore,
            (
              SELECT `MapleVersionId`
              FROM `MapleVersions` AS c
              WHERE c.`Id` = (
                SELECT `MapleVersionId`
                FROM `VersionPathHashes` b
                WHERE b.`Id` = a.`ResolvesTo`)
            ) HasntChangedSince
            FROM `VersionPathHashes` a
            WHERE
              `ImgName` IS NOT NULL 
              AND (a.`ResolvesTo` IS NULL OR (SELECT `MapleVersionId` FROM `VersionPathHashes` b WHERE b.`Id` = a.`ResolvesTo`) IN ({0}))
              AND `MapleVersionId` = @originalVersion;";

        public DiffingController(ApplicationDbContext dbCtx) => _ctx = dbCtx;
        private readonly ApplicationDbContext _ctx;

        [Route("{otherVersion}")]
        [HttpGet]
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
                    bool existedPrior = ((long)reader[1]) > 0;
                    string hasntChangedSince = reader.IsDBNull(2) ? null : (string)reader[2];
                    diffEntries.Add(new DiffEntry()
                    {
                        ExistedPrior = existedPrior,
                        HasntChangedSince = int.TryParse(hasntChangedSince, out int hasntChangedSinceVerNum) ? (int?)hasntChangedSinceVerNum : null,
                        Path = path
                    });
                }
            }

            return Json(diffEntries);
        }

        public class DiffEntry
        {
            public string Path;
            public bool ExistedPrior;
            public int? HasntChangedSince;
        }
    }
}
