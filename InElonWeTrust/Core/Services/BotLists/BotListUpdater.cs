﻿using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using InElonWeTrust.Core.Settings;
using Newtonsoft.Json;
using NLog;

namespace InElonWeTrust.Core.Services.BotLists.CommonBotLists
{
    public class BotListUpdater
    {
        public string Link { get; }
        public string CountFieldName { get; }
        public string Token { get; }

        private readonly HttpClient _httpClient;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public BotListUpdater(string link, string countFieldName, string token)
        {
            Link = link;
            CountFieldName = countFieldName;
            Token = token;

            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(Link);
            _httpClient.Timeout = new TimeSpan(0, 0, 0, 5);
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", Token);
        }

        public async Task UpdateStatus(int guildsCount)
        {
            var json = $"{{ \"{CountFieldName}\": {guildsCount} }}";

            var requestContent = new StringContent(json, Encoding.UTF8, "application/json");
            var link = string.Format(Link, SettingsLoader.Data.BotId);

            var result = await _httpClient.PostAsync(link, requestContent);
            if (!result.IsSuccessStatusCode)
            {
                //_logger.Error("Can't update bot list: " + link);
            }
        }
    }
}