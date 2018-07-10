using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tweetinvi.Models;

namespace InElonWeTrust.Core.Database.Models
{
    public class CachedTweet
    {
        public long Id { get; set; }
        public string CreatedByRealName { get; set; }
        public string CreatedByDisplayName { get; set; }
        public string AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public string FullText { get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }

        public CachedTweet()
        {

        }

        public CachedTweet(ITweet tweet)
        {
            Id = tweet.Id;
            CreatedByRealName = tweet.CreatedBy.ScreenName;
            CreatedByDisplayName = tweet.CreatedBy.Name;
            AvatarUrl = tweet.CreatedBy.ProfileImageUrl400x400;
            CreatedAt = tweet.CreatedAt;
            FullText = tweet.FullText;
            Url = tweet.Url;
            ImageUrl = tweet.Media.Count > 0 ? tweet.Media.First().MediaURL : null;
        }
    }
}
