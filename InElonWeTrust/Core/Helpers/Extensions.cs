using System;
using System.Collections.Generic;

namespace InElonWeTrust.Core.Helpers
{
    public static class Extensions
    {
        private static readonly Random Random;

        static Extensions()
        {
            Random = new Random();
        }

        public static T GetRandomItem<T>(this List<T> enumerable)
        {
            var index = Random.Next(0, enumerable.Count - 1);
            return enumerable[index];
        }

        public static string ShortenString(this string str, int maxLength)
        {
            return str.Length > maxLength ? str.Substring(0, maxLength).Insert(maxLength, "...") : str;
        }

        public static long NextLong(this Random random, long min, long max)
        {
            var buf = new byte[8];
            random.NextBytes(buf);

            var longRand = BitConverter.ToInt64(buf, 0);
            return Math.Abs(longRand % (max - min)) + min;
        }

        public static DateTime UnixTimeStampToDateTime(this DateTime dateTime, double unixTimeStamp)
        {
            var convertedDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            convertedDateTime = convertedDateTime.AddSeconds(unixTimeStamp).ToLocalTime();

            return convertedDateTime;
        }

        public static string ConvertToYesNo(this bool b, bool capitalize = true)
        {
            return capitalize ?
               (b ? "Yes" : "No") :
               (b ? "yes" : "no");
        }
    }
}
