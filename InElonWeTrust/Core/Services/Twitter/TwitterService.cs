using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Database.Models;
using InElonWeTrust.Core.Services.Subscriptions;
using InElonWeTrust.Core.Settings;
using Microsoft.EntityFrameworkCore;
using NLog;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace InElonWeTrust.Core.Services.Twitter
{
    public class TwitterService
    {
        public event EventHandler<List<CachedTweet>> OnNewTweets;

        private readonly System.Timers.Timer _tweetsUpdateTimer;
        private readonly Dictionary<TwitterUserType, string> _users;
        private readonly Dictionary<TwitterUserType, SubscriptionType> _userSubscriptionMap;
        private readonly SemaphoreSlim _updateSemaphore = new SemaphoreSlim(1);

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private const int UpdateNotificationsIntervalMinutes = 1;

        public TwitterService()
        {
            _users = new Dictionary<TwitterUserType, string>
            {
                {TwitterUserType.ElonMusk, "elonmusk"},
                {TwitterUserType.SpaceX, "SpaceX"},
                {TwitterUserType.SpaceXFleet, "SpaceXFleet"}
            };

            _userSubscriptionMap = new Dictionary<TwitterUserType, SubscriptionType>
            {
                {TwitterUserType.ElonMusk, SubscriptionType.ElonTwitter},
                {TwitterUserType.SpaceX, SubscriptionType.SpaceXTwitter},
                {TwitterUserType.SpaceXFleet, SubscriptionType.SpaceXFleetTwitter}
            };

            var consumerKey = SettingsLoader.Data.TwitterConsumerKey;
            var consumerSecret = SettingsLoader.Data.TwitterConsumerSecret;
            var accessToken = SettingsLoader.Data.TwitterAccessToken;
            var accessTokenSecret = SettingsLoader.Data.TwitterAccessTokenSecret;

            Auth.SetUserCredentials(consumerKey, consumerSecret, accessToken, accessTokenSecret);

            _tweetsUpdateTimer = new System.Timers.Timer(UpdateNotificationsIntervalMinutes * 60 * 1000);
            _tweetsUpdateTimer.Elapsed += TweetRangesUpdateTimer_ElapsedAsync;
            _tweetsUpdateTimer.Start();
        }

        public SubscriptionType GetSubscriptionTypeByUserName(string username)
        {
            var userType = _users.First(p => p.Value == username).Key;
            return _userSubscriptionMap[userType];
        }

        public async Task<CachedTweet> GetRandomTweetAsync(TwitterUserType userType)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var username = _users[userType];
                return await databaseContext.CachedTweets.Where(p => p.CreatedByRealName == username).OrderBy(r => Guid.NewGuid()).FirstAsync();
            }
        }

        public string GetAvatar(TwitterUserType userType)
        {
            return User.GetUserFromScreenName(_users[userType]).ProfileImageUrlFullSize;
        }

        public async Task ReloadCachedTweetsAsync(bool checkOnlyLastTweets)
        {
            if (!await _updateSemaphore.WaitAsync(0))
            {
                return;
            }

            try
            {
                using (var databaseContext = new DatabaseContext())
                {
                    _logger.Info("Twitter reload cached tweets starts");

                    var sendNotifyWhenNewTweet = await databaseContext.CachedTweets.AnyAsync();
                    var sentTweets = 0;

                    foreach (var account in _users)
                    {
                        var allTweets = GetAllTweets(account.Value, checkOnlyLastTweets);
                        var cachedTweetsToSend = GetTweetsToSend(allTweets);
                        await AddTweetsToDatabase(cachedTweetsToSend);

                        if (sendNotifyWhenNewTweet)
                        {
                            OnNewTweets?.Invoke(account, cachedTweetsToSend);
                            sentTweets += cachedTweetsToSend.Count;
                        }

                        _logger.Info($"Twitter user ({account.Value}) done");
                    }

                    await databaseContext.SaveChangesAsync();

                    var tweetsCount = await databaseContext.CachedTweets.CountAsync();
                    _logger.Info($"Twitter update finished ({sentTweets} sent, {tweetsCount} tweets in database)");
                }
            }
            finally
            {
                _updateSemaphore.Release();
            }
        }

        private List<ITweet> GetAllTweets(string username, bool onlyOnePage)
        {
            var firstRequest = true;
            var maxTweetId = long.MaxValue;
            var tweets = new List<ITweet>();

            while (true)
            {
                var retrievedTweets = Timeline.GetUserTimeline(username, new UserTimelineParameters
                {
                    MaxId = firstRequest ? -1 : maxTweetId - 1,
                    MaximumNumberOfTweetsToRetrieve = 200
                })?.ToList();

                tweets.AddRange(retrievedTweets);

                if (onlyOnePage || retrievedTweets == null || !retrievedTweets.Any())
                {
                    break;
                }

                maxTweetId = Math.Min(maxTweetId, retrievedTweets.Min(p => p.Id));
                firstRequest = false;
            }

            return tweets;
        }

        private List<CachedTweet> GetTweetsToSend(List<ITweet> tweets)
        {
            using (var databaseContext = new DatabaseContext())
            {
                return tweets.Where(msg => !databaseContext.CachedTweets.Any(p => p.Id == msg.Id))
                    .OrderBy(p => p.CreatedAt)
                    .Select(p => new CachedTweet(p))
                    .ToList();
            }
        }

        private async Task AddTweetsToDatabase(List<CachedTweet> cachedTweets)
        {
            using (var databaseContext = new DatabaseContext())
            {
                await databaseContext.CachedTweets.AddRangeAsync(cachedTweets);
                await databaseContext.SaveChangesAsync();
            }
        }

        private async void TweetRangesUpdateTimer_ElapsedAsync(object sender, ElapsedEventArgs e)
        {
            try
            {
                await ReloadCachedTweetsAsync(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to reload cached tweets.");
            }
        }
    }
}
