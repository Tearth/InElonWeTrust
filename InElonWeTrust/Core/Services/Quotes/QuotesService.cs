using System;
using System.Linq;
using System.Threading.Tasks;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Helpers.Extensions;
using Microsoft.EntityFrameworkCore;

namespace InElonWeTrust.Core.Services.Quotes
{
    public class QuotesService
    {
        public string GetRandomQuoteAsync()
        {
            using (var databaseContext = new DatabaseContext())
            {
                var quote = databaseContext.Quotes.RandomRow("Quotes").First();
                return quote.Text;
            }
        }
    }
}
