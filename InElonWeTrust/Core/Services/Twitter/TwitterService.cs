using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using DSharpPlus;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Settings;
using Tweetinvi;
using Tweetinvi.Events;
using Tweetinvi.Models;
using Tweetinvi.Parameters;
using Tweetinvi.Streaming;
using Tweetinvi.Streams;

namespace InElonWeTrust.Core.Services.Twitter
{
    public class TwitterService
    {
        public event EventHandler<ITweet> OnNewTweet;

        private Dictionary<TwitterUserType, string> _users;
        private ConcurrentDictionary<TwitterUserType, List<SlimTweet>> _tweets;
        private IFilteredStream _stream;

        public TwitterService()
        {
            _tweets = new ConcurrentDictionary<TwitterUserType, List<SlimTweet>>();

            _users = new Dictionary<TwitterUserType, string>
            {
                {TwitterUserType.ElonMusk, "elonmusk"},
                {TwitterUserType.SpaceX, "SpaceX"}
            };

            var consumerKey = SettingsLoader.Data.TwitterConsumerKey;
            var consumerSecret = SettingsLoader.Data.TwitterConsumerSecret;
            var accessToken = SettingsLoader.Data.TwitterAccessToken;
            var accessTokenSecret = SettingsLoader.Data.TwitterAccessTokenSecret;

            Auth.SetUserCredentials(consumerKey, consumerSecret, accessToken, accessTokenSecret);

            Task.Run(() => DownloadAllTweets());
            InitStream();
        }

        private void InitStream()
        {
            _stream = Stream.CreateFilteredStream();

            foreach (var user in _users)
            {
                _stream.AddFollow(User.GetUserFromScreenName(user.Value));
            }

            _stream.MatchingTweetReceived += Stream_MatchingTweetReceived;
            _stream.StartStreamMatchingAllConditionsAsync();
        }

        private void Stream_MatchingTweetReceived(object sender, MatchedTweetReceivedEventArgs e)
        {
            if (e.MatchOn == MatchOn.Follower)
            {
                var userType = _users.First(p => p.Value == e.Tweet.CreatedBy.ScreenName).Key;
                _tweets[userType].Add(new SlimTweet(e.Tweet));

                OnNewTweet?.Invoke(e.Tweet.CreatedBy, e.Tweet);
            }
        }

        public SlimTweet GetRandomTweet(TwitterUserType userType)
        {
            var tweets = _tweets[userType];
            return tweets.GetRandomItem();
        }

        private void DownloadAllTweets()
        {
            Bot.Client.DebugLogger.LogMessage(LogLevel.Info, Constants.AppName, "Twitter download start", DateTime.Now);

            Parallel.ForEach(_users, account =>
            {
                _tweets.TryAdd(account.Key, new List<SlimTweet>());

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

                    _tweets[account.Key].AddRange(messages.Select(p => new SlimTweet(p)));

                    minTweetId = Math.Min(minTweetId, messages.Min(p => p.Id));
                    firstRequest = false;
                }
            });

            var tweetsCount = _tweets.Sum(p => p.Value.Count);
            Bot.Client.DebugLogger.LogMessage(LogLevel.Info, Constants.AppName, $"Twitter download finished ({tweetsCount} tweets loaded)", DateTime.Now);
        }
    }
}
