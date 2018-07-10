using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using DSharpPlus;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Database.Models;
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
        private IFilteredStream _stream;

        public TwitterService()
        {
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

        public CachedTweet GetRandomTweet(TwitterUserType userType)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var username = _users[userType];
                return databaseContext.CachedTweets.Where(p => p.CreatedByRealName == username).OrderBy(r => Guid.NewGuid()).First();
            }
        }

        public void ReloadCachedTweets()
        {
            using (var databaseContext = new DatabaseContext())
            {
                Bot.Client.DebugLogger.LogMessage(LogLevel.Info, Constants.AppName, "Twitter download start", DateTime.Now);

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

                        foreach (var msg in messages.Where(msg => !databaseContext.CachedTweets.Any(p => p.ID == msg.Id)))
                        {
                            databaseContext.CachedTweets.Add(new CachedTweet(msg));
                        }

                        minTweetId = Math.Min(minTweetId, messages.Min(p => p.Id));
                        firstRequest = false;
                    }

                    Bot.Client.DebugLogger.LogMessage(LogLevel.Info, Constants.AppName, $"Twitter user ({account.Value}) done", DateTime.Now);
                }

                databaseContext.SaveChanges();

                var tweetsCount = databaseContext.CachedTweets.Count();
                Bot.Client.DebugLogger.LogMessage(LogLevel.Info, Constants.AppName, $"Twitter download finished ({tweetsCount} tweets downloaded)", DateTime.Now);
            }
        }

        private void Stream_MatchingTweetReceived(object sender, MatchedTweetReceivedEventArgs e)
        {
            if (e.MatchOn == MatchOn.Follower)
            {
                using (var databaseContext = new DatabaseContext())
                {
                    databaseContext.CachedTweets.Add(new CachedTweet(e.Tweet));
                    databaseContext.SaveChanges();
                }

                OnNewTweet?.Invoke(e.Tweet.CreatedBy, e.Tweet);
            }
        }
    }
}
