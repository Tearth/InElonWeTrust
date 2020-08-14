using System;
using System.Globalization;
using Oddity.Models.Launches;

namespace InElonWeTrust.Core.Helpers.Formatters
{
    public static class DateFormatter
    {
        public static string GetDateStringWithPrecision(DateTime date, DatePrecision precision, bool includeUtc, bool displayPrecision, bool longDate)
        {
            string format;
            switch (precision)
            {
                case DatePrecision.Year:
                case DatePrecision.Half:
                case DatePrecision.Quarter:
                    format = "yyyy";
                    break;

                case DatePrecision.Month:
                    format = longDate ? "MMMM yyyy" : "MM-yyyy";
                    break;

                case DatePrecision.Day:
                    format = longDate ? "dddd, dd MMMM yyyy" : "dd-MM-yyyy";
                    break;

                case DatePrecision.Hour:
                    format = longDate ? "dddd, dd MMMM yyyy HH:mm" : "dd-MM-yyyy HH:mm";
                    break;

                default:
                    format = longDate ? "dddd, dd MMMM yyyy HH:mm" : "dd-MM-yyyy HH:mm";
                    break;
            }

            var output = date.ToString(format, CultureInfo.InvariantCulture);
            if (includeUtc && precision == DatePrecision.Hour)
            {
                output += " UTC";
            }

            if (displayPrecision && precision != DatePrecision.Hour)
            {
                output += $" ({precision.ToString().ToLower(CultureInfo.InvariantCulture)} precision)";
            }

            return output;
        }
    }
}
