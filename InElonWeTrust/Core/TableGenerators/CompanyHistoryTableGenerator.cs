using System.Collections.Generic;
using System.Text;
using InElonWeTrust.Core.Services.Pagination;
using Oddity.API.Models.Company;

namespace InElonWeTrust.Core.TableGenerators
{
    public class CompanyHistoryTableGenerator
    {
        private const int IdLength = 4;
        private const int DateLength = 23;
        private const int TitleLength = 45;
        private const int TotalLength = IdLength + DateLength + TitleLength;

        public string Build(List<HistoryEvent> history, int currentPage, string paginationFooter)
        {
            var historyBuilder = new StringBuilder();
            historyBuilder.Append("```");

            historyBuilder.Append("No. ".PadRight(IdLength));
            historyBuilder.Append("Date".PadRight(DateLength));
            historyBuilder.Append("Title".PadRight(TitleLength));
            historyBuilder.Append("\r\n");
            historyBuilder.Append(new string('-', TotalLength));
            historyBuilder.Append("\r\n");

            var i = (currentPage - 1) * PaginationService.ItemsPerPage + 1;

            foreach (var historyEvent in history)
            {
                historyBuilder.Append($"{i}.".PadRight(IdLength));
                historyBuilder.Append((historyEvent.EventDate?.ToString("dd-MM-yy HH:mm:ss") ?? string.Empty).PadRight(DateLength));
                historyBuilder.Append(historyEvent.Title.PadRight(TitleLength));
                historyBuilder.Append("\r\n");

                i++;
            }

            historyBuilder.Append("\r\n");
            historyBuilder.Append("Type \"e!GetEvent number\" to get more information.");

            historyBuilder.Append("\r\n");
            historyBuilder.Append(paginationFooter);
            historyBuilder.Append("```");
            return historyBuilder.ToString();
        }
    }
}
