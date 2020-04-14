using Microsoft.EntityFrameworkCore;

namespace vpn_crash.DB
{
    public class VPNCrashDB : DbContext
    {
        public DbSet<CrashEntry> CrashEntries { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=vpn-crash.db");
        }

    }
}