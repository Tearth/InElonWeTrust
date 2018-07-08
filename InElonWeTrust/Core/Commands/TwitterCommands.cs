using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Services.Twitter;
using InElonWeTrust.Core.Settings;
using Tweetinvi;
using Tweetinvi.Parameters;

namespace InElonWeTrust.Core.Commands
{
    [Commands("Twitter")]
    public class TwitterCommands
    {
        private TwitterService _twitter;

        public TwitterCommands()
        {
            _twitter = new TwitterService();
        }

        [Command("randomelontweet")]
        [Aliases("randomet", "ret")]
        [Description("Get random Elon's tweet.")]
        public async Task RandomElonTweet(CommandContext ctx)
        {
            var test = _twitter.GetRandomTweet(TwitterUserType.ElonMusk);
        }
    }
}
