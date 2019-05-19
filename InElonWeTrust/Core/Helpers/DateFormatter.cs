using System;
using System.Globalization;
using Oddity.API.Models.Launch;

namespace InElonWeTrust.Core.Helpers
{
    public static class DateFormatter
    {
        public static string GetStringWithPrecision(DateTime date, TentativeMaxPrecision precision)
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
                    format = "dddd, dd MMMM yyyy, HH:mm";
                    break;
            }

            return date.ToString(format, CultureInfo.InvariantCulture);
        }
    }
}
