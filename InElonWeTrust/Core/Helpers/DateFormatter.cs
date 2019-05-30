using System;
using System.Globalization;
using Oddity.API.Models.Launch;

namespace InElonWeTrust.Core.Helpers
{
    public static class DateFormatter
    {
        // TODO: fix this crap
        public static string GetStringWithPrecision(DateTime date, TentativeMaxPrecision precision, bool includeUTC, bool displayPrecision)
        {
            var format = string.Empty;
            switch (precision)
            {
                case TentativeMaxPrecision.Year:
                case TentativeMaxPrecision.Half:
                case TentativeMaxPrecision.Quarter:
                    format = "yyyy";
                    break;

                case TentativeMaxPrecision.Month:
                    format = "MMMM yyyy";
                    break;

                case TentativeMaxPrecision.Day:
                    format = "dddd, dd MMMM yyyy";
                    break;

                case TentativeMaxPrecision.Hour:
                    format = "dddd, dd MMMM yyyy HH:mm";
                    break;
            }

            var output = date.ToString(format, CultureInfo.InvariantCulture);
            if (includeUTC && precision == TentativeMaxPrecision.Hour)
            {
                output += " UTC";
            }

            if (displayPrecision && precision != TentativeMaxPrecision.Hour)
            {
                output += $" ({precision.ToString().ToLower(CultureInfo.InvariantCulture)} precision)";
            }

            return output;
        }

        public static string GetShortStringWithPrecision(DateTime date, TentativeMaxPrecision precision, bool includeUTC, bool displayPrecision)
        {
            var format = string.Empty;
            switch (precision)
            {
                case TentativeMaxPrecision.Year:
                case TentativeMaxPrecision.Half:
                case TentativeMaxPrecision.Quarter:
                    format = "yyyy";
                    break;

                case TentativeMaxPrecision.Month:
                    format = "MM-yyyy";
                    break;

                case TentativeMaxPrecision.Day:
                    format = "dd-MM-yyyy";
                    break;

                case TentativeMaxPrecision.Hour:
                    format = "dd-MM-yyyy HH:mm";
                    break;
            }

            var output = date.ToString(format, CultureInfo.InvariantCulture);
            if (includeUTC && precision == TentativeMaxPrecision.Hour)
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
