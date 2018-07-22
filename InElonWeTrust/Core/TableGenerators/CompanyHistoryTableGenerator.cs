using System;
using System.Collections.Generic;
using System.Text;
using InElonWeTrust.Core.Services.Pagination;
using Oddity.API.Models.Company;

namespace InElonWeTrust.Core.TableGenerators
{
    public class CompanyHistoryTableGenerator
    {
        private int _idLength = 4;
        private int _dateLength = 23;
        private int _titleLength = 45;
        private int _totalLength => _idLength + _dateLength + _titleLength;

        public string Build(List<HistoryEvent> history, int currentPage, string paginationFooter)
        {
            var historyBuilder = new StringBuilder();
            historyBuilder.Append("```");

            historyBuilder.Append("No. ".PadRight(_idLength));
            historyBuilder.Append("Date".PadRight(_dateLength));
            historyBuilder.Append("Title".PadRight(_titleLength));
            historyBuilder.Append("\r\n");
            historyBuilder.Append(new string('-', _totalLength));
            historyBuilder.Append("\r\n");

            var i = (currentPage - 1) * PaginationService.ItemsPerPage + 1;

            foreach (var historyEvent in history)
            {
                historyBuilder.Append($"{i}.".PadRight(_idLength));
                historyBuilder.Append(historyEvent.EventDate.Value.ToString("dd-MM-yy HH:mm:ss").PadRight(_dateLength));
                historyBuilder.Append(historyEvent.Title.PadRight(_titleLength));
                historyBuilder.Append("\r\n");

                i++;
            }

            historyBuilder.Append("\r\n");
            historyBuilder.Append("Type e!getevent <number> to get more information.");

            historyBuilder.Append("\r\n");
            historyBuilder.Append(paginationFooter);
            historyBuilder.Append("```");
            return historyBuilder.ToString();
        }
    }
}
