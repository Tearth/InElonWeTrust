using System.Linq;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Helpers.Extensions;

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
