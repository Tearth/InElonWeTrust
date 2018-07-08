using System.Collections.Generic;
using Newtonsoft.Json;

namespace InElonWeTrust.Core.Settings
{
    public class SettingsContainer
    {
        public string Token { get; set; }
        public List<string> Prefixes { get; set; }

        [JsonProperty("WaaAi_Token")]
        public string WaaAiToken { get; set; }

        [JsonProperty("twitter_consumer_key")]
        public string TwitterConsumerKey { get; set; }

        [JsonProperty("twitter_consumer_secret")]
        public string TwitterConsumerSecret { get; set; }

        [JsonProperty("twitter_access_token")]
        public string TwitterAccessToken { get; set; }

        [JsonProperty("twitter_access_token_secret")]
        public string TwitterAccessTokenSecret { get; set; }
    }
}