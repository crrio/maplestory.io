using maplestory.io.Entities.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace maplestory.io.Entities
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<MapleVersion> MapleVersions { get; set; }
        public DbSet<VersionPathHash> VersionPathHashes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(GetConnectionString());
        }

        internal static string GetConnectionString()
        {
            string databaseHost = Environment.GetEnvironmentVariable("MYSQL_DBHOST");
            string databaseName = Environment.GetEnvironmentVariable("MYSQL_DBNAME");
            string databaseUser = Environment.GetEnvironmentVariable("MYSQL_DBUSER");
            string databasePass = Environment.GetEnvironmentVariable("MYSQL_DBPASS");

            return $"Server={databaseHost};" +
                   $"database={databaseName};" +
                   $"uid={databaseUser};" +
                   $"pwd={databasePass};" +
                   $"pooling=true;Allow User Variables=True";
        }
    }
}
