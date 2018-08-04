﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using InElonWeTrust.Core.Settings;
using Newtonsoft.Json;
using NLog;

namespace InElonWeTrust.Core.Services.BotLists
{
    public class BotlistService
    {
        private readonly HttpClient _httpClient;
        private readonly Timer _statusRefreshTimer;

        private const int StatusUpdateIntervalMinutes = 10;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public BotlistService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://botlist.space/api/");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", SettingsLoader.Data.BotlistToken);

            _statusRefreshTimer = new Timer(StatusUpdateIntervalMinutes * 60 * 1000);
            _statusRefreshTimer.Elapsed += StatusRefreshTimer_Elapsed;
            _statusRefreshTimer.Start();
        }

        private async void StatusRefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                await UpdateStatus();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to update bot status on Botlist.space");
            }
        }

        private async Task UpdateStatus()
        {
            var guildsCount = Bot.Client.Guilds.Count;
            var requestModel = new BotlistRequest(guildsCount);

            var json = JsonConvert.SerializeObject(requestModel);

            var requestContent = new StringContent(json, Encoding.UTF8, "application/json");
            await _httpClient.PostAsync($"bots/{SettingsLoader.Data.BotId}", requestContent);
        }
    }
}
