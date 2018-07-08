using System;
using System.Collections.Generic;
using System.Text;

namespace InElonWeTrust.Core.Services.Twitter
{
    public class TweetRanges
    {
        public long MinTweetId { get; set; }
        public long MaxTweetId { get; set; }

        public TweetRanges()
        {
            Reset();
        }

        public void Reset()
        {
            MinTweetId = long.MaxValue;
            MaxTweetId = long.MinValue;
        }
    }
}
