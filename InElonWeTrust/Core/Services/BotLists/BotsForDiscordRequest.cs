using Newtonsoft.Json;

namespace InElonWeTrust.Core.Services.BotLists
{
    public class BotsForDiscordRequest
    {
        [JsonProperty("count")]
        public int Count { get; }

        public BotsForDiscordRequest(int count)
        {
            Count = count;
        }
    }
}
