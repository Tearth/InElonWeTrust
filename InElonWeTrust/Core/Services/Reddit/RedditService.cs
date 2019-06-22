using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Database.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NLog;

namespace InElonWeTrust.Core.Services.Reddit
{
    public class RedditService
    {
        public event EventHandler<List<RedditChildData>> OnNewHotTopic;
        private readonly HttpClient _httpClient;
        private readonly SemaphoreSlim _updateSemaphore = new SemaphoreSlim(1);

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private const int UpdateNotificationsIntervalMinutes = 15;
        private const string RandomTopicUrl = "random/.json";
        private const string HotTopicsUrl = "hot/.json";

        public RedditService()
        {
            _httpClient = new HttpClient {BaseAddress = new Uri("https://www.reddit.com/r/spacex/")};

            var notificationsUpdateTimer = new System.Timers.Timer(UpdateNotificationsIntervalMinutes * 60 * 1000);
            notificationsUpdateTimer.Elapsed += NotificationsUpdateTimerOnElapsedAsync;
            notificationsUpdateTimer.Start();
        }

        public async Task<RedditChildData> GetRandomTopicAsync()
        {
            var response = await _httpClient.GetStringAsync(RandomTopicUrl);

            var parsedResponse = JsonConvert.DeserializeObject<List<RedditResponse>>(response);
            return parsedResponse.First().Data.Children.First().Data;
        }

        public async Task ReloadCachedTopicsAsync()
        {
            if (!await _updateSemaphore.WaitAsync(0))
            {
                return;
            }

            try
            {
                using (var databaseContext = new DatabaseContext())
                {
                    _logger.Info("Reddit topics check starts");

                    var sendNotifies = await databaseContext.CachedRedditTopics.AnyAsync();
                    var allHotThreads = await GetAllHotThreadsNames();
                    var hotThreadsToSend = GetHotThreadsNamesToSend(allHotThreads);
                    await AddHotThreadsToDatabase(hotThreadsToSend);

                    if (sendNotifies && hotThreadsToSend.Count > 0)
                    {
                        OnNewHotTopic?.Invoke(this, hotThreadsToSend);
                    }

                    var topicsCount = databaseContext.CachedRedditTopics.Count();
                    _logger.Info($"Reddit update finished ({hotThreadsToSend.Count} sent, {topicsCount} topics in database)");
                }
            }
            finally
            {
                _updateSemaphore.Release();
            }
        }

        private async void NotificationsUpdateTimerOnElapsedAsync(object sender, ElapsedEventArgs elapsedEventArgs)
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

        private async Task<List<RedditChildData>> GetAllHotThreadsNames()
        {
            var response = await _httpClient.GetStringAsync(HotTopicsUrl);
            var hotThreads = JsonConvert.DeserializeObject<RedditResponse>(response).Data.Children.Select(p => p.Data).ToList();

            hotThreads.Reverse();
            return hotThreads;
        }

        private List<RedditChildData> GetHotThreadsNamesToSend(List<RedditChildData> hotThreads)
        {
            using (var databaseContext = new DatabaseContext())
            {
                return hotThreads.Where(newHotThread => !databaseContext.CachedRedditTopics.Any(threadInDb => threadInDb.Name == newHotThread.Name)).ToList();
            }
        }

        private async Task AddHotThreadsToDatabase(List<RedditChildData> hotThreads)
        {
            using (var databaseContext = new DatabaseContext())
            {
                await databaseContext.CachedRedditTopics.AddRangeAsync(hotThreads.Select(p => new CachedRedditTopic(p.Name)));
                await databaseContext.SaveChangesAsync();
            }
        }
    }
}
