using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using InElonWeTrust.Core.Settings;
using Newtonsoft.Json;

namespace InElonWeTrust.Core.Services.BotLists.CommonBotLists
{
    public class BotListUpdater
    {
        public string Link { get; }
        public string Token { get; }

        private readonly HttpClient _httpClient;

        public BotListUpdater(string link, string token)
        {
            Link = link;
            Token = token;

            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(Link);
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", Token);
        }

        public async Task UpdateStatus(int guildsCount)
        {
            var requestModel = new CommonBotListsRequest(guildsCount);
            var json = JsonConvert.SerializeObject(requestModel);

            var requestContent = new StringContent(json, Encoding.UTF8, "application/json");
            var link = string.Format(Link, SettingsLoader.Data.BotId);

            await _httpClient.PostAsync(link, requestContent);
        }
    }
}
