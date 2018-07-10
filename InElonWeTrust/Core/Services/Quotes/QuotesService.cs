using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InElonWeTrust.Core.Database;
using Microsoft.EntityFrameworkCore;

namespace InElonWeTrust.Core.Services.Quotes
{
    public class QuotesService
    {
        public async Task<string> GetRandomQuoteAsync()
        {
            using (var databaseContext = new DatabaseContext())
            {
                return (await databaseContext.Quotes.OrderBy(p => Guid.NewGuid()).FirstAsync()).Text;
            }
        }
    }
}
