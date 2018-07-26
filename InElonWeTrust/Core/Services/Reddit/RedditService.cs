﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Database.Models;
using Newtonsoft.Json;
using NLog;

namespace InElonWeTrust.Core.Services.Reddit
{
    public class RedditService
    {
        public event EventHandler<RedditChildData> OnNewHotTopic;
        private HttpClient _httpClient;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private const int UpdateNotificationsIntervalMinutes = 15;
        private const string RandomTopicUrl = "random/.json";
        private const string HotTopicsUrl = "hot/.json";

        public RedditService()
        {
            _httpClient = new HttpClient {BaseAddress = new Uri("https://www.reddit.com/r/spacex/")};

            var notificationsUpdateTimer = new Timer(UpdateNotificationsIntervalMinutes * 60 * 1000);
            notificationsUpdateTimer.Elapsed += NotificationsUpdateTimerOnElapsed;
            notificationsUpdateTimer.Start();
        }

        public async Task<RedditChildData> GetRandomTopic()
        {
            var response = await _httpClient.GetStringAsync(RandomTopicUrl);

            var parsedResponse = JsonConvert.DeserializeObject<List<RedditResponse>>(response);
            return parsedResponse.First().Data.Children.First().Data;
        }

        public async Task ReloadCachedTopicsAsync()
        {
            using (var databaseContext = new DatabaseContext())
            {
                _logger.Info("Reddit topics check starts");

                var sendNotifies = databaseContext.CachedRedditTopics.Any();
                var response = await _httpClient.GetStringAsync(HotTopicsUrl);

                var names = JsonConvert.DeserializeObject<RedditResponse>(response);
                foreach (var topic in names.Data.Children.Select(p => p.Data))
                {
                    if (!databaseContext.CachedRedditTopics.Any(p => p.Name == topic.Name))
                    {
                        if (sendNotifies)
                        {
                            OnNewHotTopic?.Invoke(this, topic);
                        }

                        databaseContext.CachedRedditTopics.Add(new CachedRedditTopic(topic.Name));
                    }
                }

                await databaseContext.SaveChangesAsync();
                _logger.Info($"Reddit topics check end ({databaseContext.CachedRedditTopics.Count()} cached)");
            }
        }

        private async void NotificationsUpdateTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            try
            {
                await ReloadCachedTopicsAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Can't refresh Reddit topics.");
            }
        }
    }
}
