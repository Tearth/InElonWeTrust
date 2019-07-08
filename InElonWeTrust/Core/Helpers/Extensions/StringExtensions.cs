namespace InElonWeTrust.Core.Helpers.Extensions
{
    public static class StringExtensions
    {
        public static string ShortenString(this string str, int maxLength)
        {
            if (str == null)
            {
                return null;
            }

            return str.Length > maxLength ? str.Substring(0, maxLength - 3).Insert(maxLength, "...") : str;
        }
    }
}
