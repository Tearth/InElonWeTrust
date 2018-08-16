using Newtonsoft.Json;

namespace InElonWeTrust.Core.Services.BotLists
{
    public class CommonBotListsRequest
    {
        [JsonProperty("server_count")]
        public int ServerCount { get; }

        public CommonBotListsRequest(int serverCount)
        {
            ServerCount = serverCount;
        }
    }
}
