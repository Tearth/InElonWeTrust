using System;
using System.Globalization;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Reddit;
using InElonWeTrust.Core.Services.Subscriptions;

namespace InElonWeTrust.Core.Commands
{
    [Commands(":registered:", "Reddit", "Reddit stuff")]
    public class RedditCommands
    {
        private RedditService _reddit;
        private SubscriptionsService _subscriptions;

        public RedditCommands()
        {
            _reddit = new RedditService();
            _subscriptions = new SubscriptionsService();
        }

        [Command("RandomRedditTopic")]
        [Aliases("RandomReddit", "RandomTopic", "rt")]
        [Description("Get random Reddit topic from /s/spacex.")]
        public async Task GetElonQuote(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var topic = await _reddit.GetRandomTopic();
            await DisplayTopic(ctx, topic);
        }

        private async Task DisplayTopic(CommandContext ctx, RedditChildData topic)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor),
                Title = topic.Title,
                Url = "https://www.reddit.com" + topic.Permalink,
                ThumbnailUrl = topic.Thumbnail == "self" || topic.Thumbnail == "default" ? Constants.SpaceXLogoImage : topic.Thumbnail
            };

            var contentBuilder = new StringBuilder();
            contentBuilder.Append($"{topic.Author} | {topic.Upvotes} upvotes\r\n");
            contentBuilder.Append(new DateTime().UnixTimeStampToDateTime(topic.Created).ToString("F", CultureInfo.InvariantCulture));

            embed.AddField($"-------------------", contentBuilder.ToString());

            await ctx.RespondAsync("", false, embed);
        }
    }
}
