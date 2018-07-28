using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Database.Models;
using InElonWeTrust.Core.Services.Subscriptions;
using InElonWeTrust.Core.Settings;
using Microsoft.EntityFrameworkCore;
using NLog;
using Tweetinvi;
using Tweetinvi.Events;
using Tweetinvi.Models;
using Tweetinvi.Parameters;
using Tweetinvi.Streaming;

namespace InElonWeTrust.Core.Services.Twitter
{
    public class TwitterService
    {
        public event EventHandler<ITweet> OnNewTweet;

        private readonly Dictionary<TwitterUserType, string> _users;
        private readonly Dictionary<TwitterUserType, SubscriptionType> _userSubscriptionMap;
        private IFilteredStream _stream;
        private bool _reloadingCacheState;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public TwitterService()
        {
            _users = new Dictionary<TwitterUserType, string>
            {
                {TwitterUserType.ElonMusk, "elonmusk"},
                {TwitterUserType.SpaceX, "SpaceX"}
            };

            _userSubscriptionMap = new Dictionary<TwitterUserType, SubscriptionType>
            {
                {TwitterUserType.ElonMusk, SubscriptionType.ElonTwitter},
                {TwitterUserType.SpaceX, SubscriptionType.SpaceXTwitter}
            };

            var consumerKey = SettingsLoader.Data.TwitterConsumerKey;
            var consumerSecret = SettingsLoader.Data.TwitterConsumerSecret;
            var accessToken = SettingsLoader.Data.TwitterAccessToken;
            var accessTokenSecret = SettingsLoader.Data.TwitterAccessTokenSecret;

            Auth.SetUserCredentials(consumerKey, consumerSecret, accessToken, accessTokenSecret);
            InitStream();
        }

        private void InitStream()
        {
            _stream = Stream.CreateFilteredStream();
            _stream.StallWarnings = true;

            foreach (var user in _users)
            {
                _stream.AddFollow(User.GetUserFromScreenName(user.Value));
            }

            _stream.MatchingTweetReceived += Stream_MatchingTweetReceived;
            _stream.DisconnectMessageReceived += Stream_DisconnectMessageReceived;
            _stream.WarningFallingBehindDetected += Stream_WarningFallingBehindDetected;
            _stream.StartStreamMatchingAllConditionsAsync();
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

        public async Task ReloadCachedTweetsAsync()
        {
            if (_reloadingCacheState)
            {
                return;
            }

            _reloadingCacheState = true;
            try
            {
                using (var databaseContext = new DatabaseContext())
                {
                    _logger.Info("Twitter reload cached tweets starts");

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
                            });

                            if (!messages.Any())
                            {
                                break;
                            }

                            foreach (var msg in messages.Where(msg => !databaseContext.CachedTweets.Any(p => p.Id == msg.Id)))
                            {
                                await databaseContext.CachedTweets.AddAsync(new CachedTweet(msg));
                            }

                            minTweetId = Math.Min(minTweetId, messages.Min(p => p.Id));
                            firstRequest = false;
                        }

                        _logger.Info($"Twitter user ({account.Value}) done");
                    }

                    await databaseContext.SaveChangesAsync();

                    var tweetsCount = await databaseContext.CachedTweets.CountAsync();
                    _logger.Info($"Twitter download finished ({tweetsCount} tweets downloaded)");
                }
            }
            finally
            {
                _reloadingCacheState = false;
            }
        }

        private async void Stream_MatchingTweetReceived(object sender, MatchedTweetReceivedEventArgs e)
        {
            if (e.MatchOn == MatchOn.Follower)
            {
                using (var databaseContext = new DatabaseContext())
                {
                    await databaseContext.CachedTweets.AddAsync(new CachedTweet(e.Tweet));
                    await databaseContext.SaveChangesAsync();
                }

                OnNewTweet?.Invoke(e.Tweet.CreatedBy, e.Tweet);
            }
        }

        private void Stream_DisconnectMessageReceived(object sender, DisconnectedEventArgs e)
        {
            try
            {
                _logger.Warn("Twitter stream disconnected! Reason: " + e.DisconnectMessage.Reason);

                _stream.MatchingTweetReceived -= Stream_MatchingTweetReceived;
                _stream.DisconnectMessageReceived -= Stream_DisconnectMessageReceived;
                _stream.WarningFallingBehindDetected -= Stream_WarningFallingBehindDetected;
                _stream.StopStream();

                InitStream();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Can't reconnect to Twiter stream!");
            }
        }

        private void Stream_WarningFallingBehindDetected(object sender, WarningFallingBehindEventArgs e)
        {
            _logger.Error("Stream falling behind detected: " + e.WarningMessage.Message);
        }
    }
}
