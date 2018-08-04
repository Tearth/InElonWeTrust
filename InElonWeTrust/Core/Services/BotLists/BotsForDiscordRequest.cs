using Newtonsoft.Json;

namespace InElonWeTrust.Core.Services.BotLists
{
    public class BotsForDiscordRequest
    {
        [JsonProperty("count")]
        public int ServerCount { get; }

        public BotsForDiscordRequest(int serverCount)
        {
            ServerCount = serverCount;
        }
    }
}
