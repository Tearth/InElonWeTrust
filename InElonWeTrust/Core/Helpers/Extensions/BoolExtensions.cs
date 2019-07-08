namespace InElonWeTrust.Core.Helpers.Extensions
{
    public static class BoolExtensions
    {
        public static string ConvertToYesNo(this bool b, bool capitalize = true)
        {
            return capitalize ?
                (b ? "Yes" : "No") :
                (b ? "yes" : "no");
        }
    }
}
