using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace InElonWeTrust.Core.Services.BotLists
{
    public class BotlistRequest
    {
        [JsonProperty("server_count")]
        public int ServerCount { get; }

        public BotlistRequest(int serverCount)
        {
            ServerCount = serverCount;
        }
    }
}
