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
        public event EventHandler<ITweet> OnNewTweet;

        private readonly Timer _tweetsUpdateTimer;
        private readonly Dictionary<TwitterUserType, string> _users;
        private readonly Dictionary<TwitterUserType, SubscriptionType> _userSubscriptionMap;
        private object _updatingMonitor = new object();

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private const int UpdateNotificationsIntervalMinutes = 5;

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

            _tweetsUpdateTimer = new Timer(UpdateNotificationsIntervalMinutes * 60 * 1000);
            _tweetsUpdateTimer.Elapsed += TweetRangesUpdateTimer_Elapsed;
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
            if (!Monitor.TryEnter(_updatingMonitor))
            {
                return;
            }

            try
            {
                using (var databaseContext = new DatabaseContext())
                {
                    _logger.Info("Twitter reload cached tweets starts");

                    var sendNotifyWhenNewTweet = databaseContext.CachedTweets.Any();
                    var newTweets = 0;

                    foreach (var account in _users)
                    {
                        var firstRequest = true;
                        var minTweetId = long.MaxValue;

                        while (true)
                        {
                            var messages = Timeline.GetUserTimeline(account.Value, new UserTimelineParameters
                            {
                                MaxId = firstRequest ? -1 : minTweetId - 1,
                                MaximumNumberOfTweetsToRetrieve = 200
                            })?.ToList();

                            if (messages == null || !messages.Any())
                            {
                                break;
                            }

                            foreach (var msg in messages.Where(msg => !databaseContext.CachedTweets.Any(p => p.Id == msg.Id)).OrderBy(p => p.CreatedAt))
                            {
                                await databaseContext.CachedTweets.AddAsync(new CachedTweet(msg));
                                newTweets++;

                                if (sendNotifyWhenNewTweet)
                                {
                                    OnNewTweet?.Invoke(msg.CreatedBy, msg);
                                }
                            }

                            if (checkOnlyLastTweets)
                            {
                                break;
                            }

                            minTweetId = Math.Min(minTweetId, messages.Min(p => p.Id));
                            firstRequest = false;
                        }

                        _logger.Info($"Twitter user ({account.Value}) done");
                    }

                    await databaseContext.SaveChangesAsync();

                    var tweetsCount = await databaseContext.CachedTweets.CountAsync();
                    _logger.Info($"Twitter update finished ({newTweets} sent, {tweetsCount} tweets in database)");
                }
            }
            finally
            {
                Monitor.Exit(_updatingMonitor);
            }
        }

        private async void TweetRangesUpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
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
