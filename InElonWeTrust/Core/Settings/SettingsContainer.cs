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
    }
}
