using System;

namespace InElonWeTrust.Core.Helpers.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime UnixTimeStampToDateTime(this DateTime dateTime, double unixTimeStamp)
        {
            var convertedDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            convertedDateTime = convertedDateTime.AddSeconds(unixTimeStamp);

            return convertedDateTime;
        }
    }
}
