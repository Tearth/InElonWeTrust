using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Oddity.Models.Cores;

namespace InElonWeTrust.Core.TableGenerators
{
    public class CoresListTableGenerator
    {
        private const int SerialLength = 10;
        private const int BlockLength = 7;
        private const int OriginalLaunchLength = 35;
        private const int MissionsCountLength = 10;
        private const int StatusLength = 10;
        private const int TotalLength = SerialLength + BlockLength + OriginalLaunchLength + MissionsCountLength + StatusLength;

        public string Build(List<CoreInfo> cores, int currentPage, string paginationFooter)
        {
            var historyBuilder = new StringBuilder();
            historyBuilder.Append("```");

            historyBuilder.Append("Serial".PadRight(SerialLength));
            historyBuilder.Append("Block".PadRight(BlockLength));
            historyBuilder.Append("First launch".PadRight(OriginalLaunchLength));
            historyBuilder.Append("Missions".PadRight(MissionsCountLength));
            historyBuilder.Append("Status".PadRight(StatusLength));
            historyBuilder.Append("\r\n");
            historyBuilder.Append(new string('-', TotalLength));
            historyBuilder.Append("\r\n");

            foreach (var core in cores)
            {
                var firstLaunch = core.Launches.Count > 0 ? core.Launches[0].Value : null;

                historyBuilder.Append(core.Serial.PadRight(SerialLength));
                historyBuilder.Append((core.Block?.ToString() ?? "none").PadRight(BlockLength));
                historyBuilder.Append((firstLaunch.DateUtc?.ToString("D", CultureInfo.InvariantCulture) ?? "none").PadRight(OriginalLaunchLength));
                historyBuilder.Append(core.Launches.Count.ToString().PadRight(MissionsCountLength));
                historyBuilder.Append(core.Status.ToString().PadRight(StatusLength));
                historyBuilder.Append("\r\n");
            }

            historyBuilder.Append("\r\n");
            historyBuilder.Append("Type \"e!GetCore serial\" (e.g. e!GetCore B1050) to get more information.");

            historyBuilder.Append("\r\n");
            historyBuilder.Append(paginationFooter);
            historyBuilder.Append("```");
            return historyBuilder.ToString();
        }
    }
}
