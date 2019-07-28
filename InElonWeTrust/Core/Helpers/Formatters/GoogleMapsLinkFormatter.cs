using System.Globalization;

namespace InElonWeTrust.Core.Helpers.Formatters
{
    public static class GoogleMapsLinkFormatter
    {
        public static string GetGoogleMapsLink(float latitude, float longitude)
        {
            var numberFormat = new NumberFormatInfo {NumberDecimalSeparator = "."};
            var latitudeString = latitude.ToString(numberFormat);
            var longitudeString = longitude.ToString(numberFormat);

            return $"https://maps.google.com/maps?q={latitudeString},{longitudeString}&t=k";
        }
    }
}
