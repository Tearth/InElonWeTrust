using System.Collections.Generic;
using Newtonsoft.Json;

namespace InElonWeTrust.Core.Settings
{
    public class SettingsContainer
    {
        public string Token { get; set; }
        public List<string> Prefixes { get; set; }

        [JsonProperty("bot_id")]
        public ulong BotId { get; set; }

        [JsonProperty("owner_id")]
        public ulong OwnerId { get; set; }

        [JsonProperty("twitter_consumer_key")]
        public string TwitterConsumerKey { get; set; }

        [JsonProperty("twitter_consumer_secret")]
        public string TwitterConsumerSecret { get; set; }

        [JsonProperty("twitter_access_token")]
        public string TwitterAccessToken { get; set; }

        [JsonProperty("twitter_access_token_secret")]
        public string TwitterAccessTokenSecret { get; set; }

        [JsonProperty("flickr_key")]
        public string FlickrKey { get; set; }

        [JsonProperty("flickr_secret")]
        public string FlickrSecret { get; set; }

        [JsonProperty("discord_bot_list_token")]
        public string DiscordBotListToken { get; set; }

        [JsonProperty("bots_for_discord_token")]
        public string BotsForDiscordToken { get; set; }

        [JsonProperty("botlist_token")]
        public string BotlistToken { get; set; }

        [JsonProperty("discord_pw_token")]
        public string DiscordPwToken { get; set; }
    }
}