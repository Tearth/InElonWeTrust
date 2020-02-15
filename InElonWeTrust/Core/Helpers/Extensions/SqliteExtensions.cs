using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace InElonWeTrust.Core.Helpers.Extensions
{
    public static class SqliteExtensions
    {
        public static IQueryable<T> RandomRow<T>(this DbSet<T> dbSet, string tableName, string criteria = null) where T : class
        {
            var whereClause = criteria == null ? "" : $"WHERE {criteria}";
            return dbSet.FromSqlRaw($"SELECT * FROM {tableName} {whereClause} ORDER BY RANDOM() LIMIT 1");
        }
    }
}
