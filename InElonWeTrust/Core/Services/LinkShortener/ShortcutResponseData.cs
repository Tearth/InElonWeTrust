using Newtonsoft.Json;

namespace InElonWeTrust.Core.Services.LinkShortener
{
    public class ShortcutResponseData
    {
        public string Url { get; set; }

        [JsonProperty("short_code")]
        public string ShortCode { get; set; }

        public string Extension { get; set; }

        [JsonProperty("delete_link")]
        public string DeleteLink { get; set; }

        [JsonProperty("delete_hash")]
        public string DeleteHash { get; set; }

        [JsonProperty("long_url")]
        public string LongUrl { get; set; }
    }
}
