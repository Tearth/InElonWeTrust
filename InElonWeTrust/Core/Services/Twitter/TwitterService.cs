using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Settings;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace InElonWeTrust.Core.Services.Twitter
{
    public class TwitterService
    {
        private Dictionary<string, TweetRanges> _tweetRanges;
        private Dictionary<TwitterUserType, string> _twitterUsers;
        private Timer _tweetRangesUpdateTimer;
        private Random _random;

        private const int InitialIntervalSeconds = 2;
        private const int IntervalMinutes = 60 * 12;

        public TwitterService()
        {
            _tweetRanges = new Dictionary<string, TweetRanges>
            {
                {"elonmusk", new TweetRanges()},
                {"SpaceX", new TweetRanges()}
            };

            _twitterUsers = new Dictionary<TwitterUserType, string>
            {
                {TwitterUserType.ElonMusk, "elonmusk"},
                {TwitterUserType.SpaceX, "SpaceX"}
            };

            _tweetRangesUpdateTimer = new Timer(InitialIntervalSeconds);
            _tweetRangesUpdateTimer.Elapsed += TweetRangesUpdateTimer_Elapsed;

            _random = new Random();

            var consumerKey = SettingsLoader.Data.TwitterConsumerKey;
            var consumerSecret = SettingsLoader.Data.TwitterConsumerSecret;
            var accessToken = SettingsLoader.Data.TwitterAccessToken;
            var accessTokenSecret = SettingsLoader.Data.TwitterAccessTokenSecret;

            Auth.SetUserCredentials(consumerKey, consumerSecret, accessToken, accessTokenSecret);
            _tweetRangesUpdateTimer.Start();
        }

        public ITweet GetRandomTweet(TwitterUserType userType)
        {
            var username = _twitterUsers[userType];

            var tweetRanges = _tweetRanges[username];
            var randomId = _random.NextLong(tweetRanges.MinTweetId, tweetRanges.MaxTweetId);

            var messages = Timeline.GetUserTimeline(username, new UserTimelineParameters
            {
                MaxId = randomId,
                MaximumNumberOfTweetsToRetrieve = 1
            });

            return messages.First();
        }

        private void TweetRangesUpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _tweetRangesUpdateTimer.Interval = IntervalMinutes * 60 * 1000;
            UpdateTweetRanges();
        }

        private void UpdateTweetRanges()
        {
            foreach (var account in _tweetRanges)
            {
                account.Value.Reset();

                var firstRequest = true;
                while (true)
                {
                    var messages = Timeline.GetUserTimeline(account.Key, new UserTimelineParameters
                    {
                        MaxId = firstRequest ? -1 : account.Value.MinTweetId - 1,
                        MaximumNumberOfTweetsToRetrieve = 200
                    });

                    if (!messages.Any())
                    {
                        break;
                    }

                    account.Value.MinTweetId = Math.Min(account.Value.MinTweetId, messages.Min(p => p.Id));
                    account.Value.MaxTweetId = Math.Max(account.Value.MaxTweetId, messages.Max(p => p.Id));

                    firstRequest = false;
                }
            }
        }
    }
}
