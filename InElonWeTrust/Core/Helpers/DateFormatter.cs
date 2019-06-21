using System;
using System.Globalization;
using Oddity.API.Models.Launch;

namespace InElonWeTrust.Core.Helpers
{
    public static class DateFormatter
    {
        public static string GetDateStringWithPrecision(DateTime date, TentativeMaxPrecision precision, bool includeUtc, bool displayPrecision, bool longDate)
        {
            string format;
            switch (precision)
            {
                case TentativeMaxPrecision.Year:
                case TentativeMaxPrecision.Half:
                case TentativeMaxPrecision.Quarter:
                    format = "yyyy";
                    break;

                case TentativeMaxPrecision.Month:
                    format = longDate ? "MMMM yyyy" : "MM-yyyy";
                    break;

                case TentativeMaxPrecision.Day:
                    format = longDate ? "dddd, dd MMMM yyyy" : "dd-MM-yyyy";
                    break;

                case TentativeMaxPrecision.Hour:
                    format = longDate ? "dddd, dd MMMM yyyy HH:mm" : "dd-MM-yyyy HH:mm";
                    break;

                default:
                    format = longDate ? "dddd, dd MMMM yyyy HH:mm" : "dd-MM-yyyy HH:mm";
                    break;
            }

            var output = date.ToString(format, CultureInfo.InvariantCulture);
            if (includeUtc && precision == TentativeMaxPrecision.Hour)
            {
                output += " UTC";
            }

            if (displayPrecision && precision != TentativeMaxPrecision.Hour)
            {
                output += $" ({precision.ToString().ToLower(CultureInfo.InvariantCulture)} precision)";
            }

            return output;
        }
    }
}
