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

        [JsonProperty("support_server_id")]
        public ulong SupportServerId { get; set; }

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

        [JsonProperty("discord_bots_token")]
        public string DiscordBotsToken { get; set; }

        [JsonProperty("bots_for_discord_token")]
        public string BotsForDiscordToken { get; set; }

        [JsonProperty("botlist_token")]
        public string BotlistToken { get; set; }

        [JsonProperty("discords_best_bots_token")]
        public string DiscordsBestBotsToken { get; set; }

        [JsonProperty("discord_bot_world_token")]
        public string DiscordBotWorldToken { get; set; }

        [JsonProperty("discord_bot_list_token")]
        public string DiscordBotListToken { get; set; }

        [JsonProperty("bots_on_discord_token")]
        public string BotsOnDiscordToken { get; set; }

        [JsonProperty("divine_discord_bots_token")]
        public string DivineDiscordBotsToken { get; set; }

        [JsonProperty("discord_bots_gg_token")]
        public string DiscordBotsGgToken { get; set; }
    }
}