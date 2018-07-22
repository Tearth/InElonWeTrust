using System;
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

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private const int IntervalMinutes = 15;

        public RedditService()
        {
            var notificationsUpdateTimer = new Timer(IntervalMinutes * 60 * 1000);
            notificationsUpdateTimer.Elapsed += NotificationsUpdateTimerOnElapsed;
            notificationsUpdateTimer.Start();
        }

        public async Task<RedditChildData> GetRandomTopic()
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync("https://www.reddit.com/r/spacex/random/.json");

            var parsedResponse = JsonConvert.DeserializeObject<List<RedditResponse>>(response);
            return parsedResponse.First().Data.Children.First().Data;
        }

        private async void NotificationsUpdateTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            using (var databaseContext = new DatabaseContext())
            {
                _logger.Info("Reddit topics check starts");

                var sendNotifies = databaseContext.CachedRedditTopics.Any();

                var httpClient = new HttpClient();
                var response = await httpClient.GetStringAsync("https://www.reddit.com/r/spacex/hot/.json");

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
    }
}
