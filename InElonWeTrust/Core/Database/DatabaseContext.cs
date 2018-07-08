using InElonWeTrust.Core.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace InElonWeTrust.Core.Database
{
    public class DatabaseContext : DbContext
    {
        public DbSet<CachedLink> CachedLinks { get; set; }
        public DbSet<PaginatedMessage> PaginatedMessages { get; set; }
        public DbSet<SubscribedChannel> SubscribedChannels { get; set; }

        public DatabaseContext() : base(GetOptions("Data Source=Database.sqlite"))
        {

        }

        private static DbContextOptions GetOptions(string connectionString)
        {
            return SqliteDbContextOptionsBuilderExtensions.UseSqlite(new DbContextOptionsBuilder(), connectionString).Options;
        }
    }
}
