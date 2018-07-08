using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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

        private Dictionary<string, List<long>> _tweetIDs;
        private Dictionary<TwitterUserType, string> _twitterUsers;
        private Timer _tweetRangesUpdateTimer;
        private Random _random;
        private IFilteredStream _stream;

        private const int InitialIntervalSeconds = 2;
        private const int IntervalMinutes = 60 * 12;

        public TwitterService()
        {
            _tweetIDs = new Dictionary<string, List<long>>();

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

            _stream = Stream.CreateFilteredStream();

            foreach (var user in _twitterUsers)
            {
                _stream.AddFollow(User.GetUserFromScreenName(user.Value));
            }

            _stream.MatchingTweetReceived += Stream_MatchingTweetReceived;
            _stream.StartStreamMatchingAllConditionsAsync();

            _tweetRangesUpdateTimer.Start();
        }

        private void Stream_MatchingTweetReceived(object sender, MatchedTweetReceivedEventArgs e)
        {
            if (e.MatchOn == MatchOn.Follower)
            {
                OnNewTweet?.Invoke(e.Tweet.CreatedBy, e.Tweet);
            }
        }

        public ITweet GetRandomTweet(TwitterUserType userType)
        {
            var username = _twitterUsers[userType];

            var tweets = _tweetIDs[username];
            var randomId = tweets.GetRandomItem();

            return Tweet.GetTweet(randomId);
        }

        private void TweetRangesUpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _tweetRangesUpdateTimer.Interval = IntervalMinutes * 60 * 1000;
            UpdateTweetRanges();
        }

        private void UpdateTweetRanges()
        {
            _tweetIDs.Clear();

            Parallel.ForEach(_twitterUsers, account =>
            {
                _tweetIDs.Add(account.Value, new List<long>());

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

                    _tweetIDs[account.Value].AddRange(messages.Select(p => p.Id));

                    minTweetId = Math.Min(minTweetId, messages.Min(p => p.Id));
                    firstRequest = false;
                }
            });
        }
    }
}
