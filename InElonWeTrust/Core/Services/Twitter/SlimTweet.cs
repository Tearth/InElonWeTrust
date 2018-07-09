using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tweetinvi.Models;

namespace InElonWeTrust.Core.Services.Twitter
{
    public class SlimTweet
    {
        public long Id { get; set; }
        public string CreatedBy { get; set; }
        public string AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public string FullText { get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }

        public SlimTweet(ITweet tweet)
        {
            Id = tweet.Id;
            CreatedBy = tweet.CreatedBy.Name;
            AvatarUrl = tweet.CreatedBy.ProfileImageUrl400x400;
            CreatedAt = tweet.CreatedAt;
            FullText = tweet.FullText;
            Url = tweet.Url;
            ImageUrl = tweet.Media.Count > 0 ? tweet.Media.First().MediaURL : null;
        }
    }
}
