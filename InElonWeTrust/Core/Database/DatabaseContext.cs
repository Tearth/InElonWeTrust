using Microsoft.EntityFrameworkCore;

namespace InElonWeTrust.Core.Database
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext() : base(GetOptions("Data Source=Database.sqlite"))
        {

        }

        private static DbContextOptions GetOptions(string connectionString)
        {
            return SqliteDbContextOptionsBuilderExtensions.UseSqlite(new DbContextOptionsBuilder(), connectionString).Options;
        }
    }
}
