namespace InElonWeTrust.Core.Services.Sn
{
    public class SnService
    {
        private readonly string _template = "That’s a shame SN{0} has RUD’d, but {2} has no doubt " +
                                            "been redesigned anyway, and I’m sure SN{1} will be along in a " +
                                            "matter of days! I have a good feeling SN{1} is the one that will " +
                                            "make the hop, no doubt in just a couple of weeks!";

        public string GetSnText()
        {
            return _template;
        }
    }
}