using System;
using System.Globalization;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.LaunchNotifications;
using InElonWeTrust.Core.Services.Reddit;
using InElonWeTrust.Core.Services.Subscriptions;
using NLog;

namespace InElonWeTrust.Core.Commands
{
    [Commands(":frame_photo:", "Media", "Commands related with Twitter, Flickr and Reddit")]
    public class RedditCommands
    {
        private RedditService _redditService;
        private SubscriptionsService _subscriptionsService;

        private Logger _logger = LogManager.GetCurrentClassLogger();

        public RedditCommands(RedditService redditService, SubscriptionsService subscriptionsService)
        {
            _redditService = redditService;
            _subscriptionsService = subscriptionsService;

            _redditService.OnNewHotTopic += Reddit_OnNewHotTopic;
        }

        [Command("RandomRedditTopic")]
        [Aliases("RandomReddit", "RandomTopic", "rrt")]
        [Description("Get random Reddit topic from /s/spacex.")]
        public async Task RandomRedditTopic(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var topic = await _redditService.GetRandomTopic();
            await DisplayTopic(ctx.Channel, topic);
        }

        private async Task DisplayTopic(DiscordChannel channel, RedditChildData topic)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor),
                Title = $"Reddit: {HttpUtility.HtmlDecode(topic.Title)}",
                Url = "https://www.reddit.com" + topic.Permalink,
                ThumbnailUrl = topic.Thumbnail == "self" || topic.Thumbnail == "default" ? Constants.SpaceXLogoImage : topic.Thumbnail
            };

            var contentBuilder = new StringBuilder();
            contentBuilder.Append($"{topic.Author} | {topic.Upvotes} upvotes\r\n");
            contentBuilder.Append(new DateTime().UnixTimeStampToDateTime(topic.Created).ToUniversalTime().ToString("F", CultureInfo.InvariantCulture) + " UTC");

            embed.AddField("\u200b", contentBuilder.ToString());

            await channel.SendMessageAsync("", false, embed);
        }

        private async void Reddit_OnNewHotTopic(object sender, RedditChildData e)
        {
            var channelIds = _subscriptionsService.GetSubscribedChannels(SubscriptionType.Reddit);
            foreach (var channelId in channelIds)
            {
                try
                {
                    var channel = await Bot.Client.GetChannelAsync(channelId);
                    await DisplayTopic(channel, e);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Can't send hot reddit topic to the channel with id {channelId}");
                }
            }
        }
    }
}
