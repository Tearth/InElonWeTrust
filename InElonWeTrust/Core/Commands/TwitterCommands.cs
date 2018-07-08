using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Twitter;
using InElonWeTrust.Core.Settings;
using Tweetinvi;
using Tweetinvi.Models;
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
            var tweet = _twitter.GetRandomTweet(TwitterUserType.ElonMusk);
            await DisplayTweet(ctx, tweet);
        }

        private async Task DisplayTweet(CommandContext ctx, ITweet tweet)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor),
                ThumbnailUrl = tweet.CreatedBy.ProfileImageUrl400x400,
                ImageUrl = tweet.Media.Count > 0 ? tweet.Media[0].MediaURL : ""
            };

            var contentBuilder = new StringBuilder();
            contentBuilder.Append(System.Web.HttpUtility.HtmlDecode(tweet.FullText));
            contentBuilder.Append("\r\n\r\n");
            contentBuilder.Append(tweet.Url);

            embed.AddField($"{tweet.CreatedBy.Name} at {tweet.CreatedAt}", contentBuilder.ToString());

            await ctx.RespondAsync("", false, embed);
        }
    }
}
