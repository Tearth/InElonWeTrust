using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Database.Models;
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Services.Cache;
using Microsoft.EntityFrameworkCore;
using NLog;
using Oddity.Models.Launches;
using Oddity.Models.Launchpads;

namespace InElonWeTrust.Core.Services.UserLaunchSubscriptions
{
    public class UserLaunchSubscriptionsService
    {
        private readonly CacheService _cacheService;
        private readonly LaunchInfoEmbedGenerator _launchInfoEmbedGenerator;
        private readonly Timer _notificationsUpdateTimer;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private bool _notified = true;

        private const int UpdateNotificationsIntervalMinutes = 1;
        private const int MinutesToLaunchToNotify = 10;
        private const string SubscribeEmojiName = ":regional_indicator_s:";

        public UserLaunchSubscriptionsService(CacheService cacheService, LaunchInfoEmbedGenerator launchInfoEmbedGenerator)
        {
            _cacheService = cacheService;
            _launchInfoEmbedGenerator = launchInfoEmbedGenerator;

            Bot.Client.MessageReactionAdded += ClientOnMessageReactionAdded;
            Bot.Client.MessageReactionRemoved += ClientOnMessageReactionRemoved;

            _notificationsUpdateTimer = new Timer(UpdateNotificationsIntervalMinutes * 60 * 1000);
            _notificationsUpdateTimer.Elapsed += Notifications_UpdateTimerOnElapsedAsync;
            _notificationsUpdateTimer.Start();
        }

        private async Task AddUserSubscription(ulong userId, ulong guildId)
        {
            var fixedUserId = userId.ToString();
            var fixedGuildId = guildId.ToString();

            var nextLaunch = await _cacheService.GetAsync<LaunchInfo>(CacheContentType.NextLaunch);

            using (var databaseContext = new DatabaseContext())
            {
                if (!await databaseContext.UserLaunchSubscriptions.AnyAsync(p => p.UserId == fixedUserId && p.LaunchId == nextLaunch.FlightNumber))
                {
                    await databaseContext.UserLaunchSubscriptions.AddAsync(new UserLaunchSubscription(nextLaunch.FlightNumber ?? uint.MinValue, fixedGuildId, fixedUserId));
                    await databaseContext.SaveChangesAsync();
                }
            }
        }

        private async Task RemoveUserSubscription(ulong userId, ulong guildId)
        {
            var fixedUserId = userId.ToString();
            var fixedGuildId = guildId.ToString();

            var nextLaunch = await _cacheService.GetAsync<LaunchInfo>(CacheContentType.NextLaunch);

            using (var databaseContext = new DatabaseContext())
            {
                var userSubscription = await databaseContext.UserLaunchSubscriptions
                    .FirstOrDefaultAsync(p => p.UserId == fixedUserId && p.GuildId == fixedGuildId && p.LaunchId == nextLaunch.FlightNumber);

                if (userSubscription != null)
                {
                    databaseContext.UserLaunchSubscriptions.Remove(userSubscription);
                    await databaseContext.SaveChangesAsync();
                }
            }
        }

        private async Task<bool> IsMessageSubscribable(ulong messageId)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var fixedMessageId = messageId.ToString();
                return await databaseContext.MessagesToSubscribe.AnyAsync(p => p.MessageId == fixedMessageId);
            }
        }

        private async Task ClientOnMessageReactionAdded(DiscordClient client, MessageReactionAddEventArgs e)
        {
            string emojiName;
            try
            {
                emojiName = e.Emoji.GetDiscordName();
            }
            catch (ArgumentNullException)
            {
                // Skip it, Discord API sometimes returns strange values
                return;
            }

            if (!e.User.IsBot && emojiName == SubscribeEmojiName && await IsMessageSubscribable(e.Message.Id))
            {
                await AddUserSubscription(e.User.Id, e.Channel.GuildId);
                _logger.Info($"User {e.User.Username} [{e.User.Id}] from {e.Channel.Guild.Name} [{e.Channel.Guild.Id}] " +
                             $"has been added to the launch subscription list");
            }
        }

        private async Task ClientOnMessageReactionRemoved(DiscordClient client, MessageReactionRemoveEventArgs e)
        {
            string emojiName;
            try
            {
                emojiName = e.Emoji.GetDiscordName();
            }
            catch (ArgumentNullException)
            {
                // Skip it, Discord API sometimes returns strange values
                return;
            }

            if (!e.User.IsBot && emojiName == SubscribeEmojiName && await IsMessageSubscribable(e.Message.Id))
            {
                await RemoveUserSubscription(e.User.Id, e.Channel.GuildId);
                _logger.Info($"User {e.User.Username} [{e.User.Id}] from {e.Channel.Guild.Name} [{e.Channel.Guild.Id}] " +
                             $"has been removed from the launch subscription list");
            }
        }

        private async Task UpdateLaunchNotifications()
        {
            var nextLaunch = await _cacheService.GetAsync<LaunchInfo>(CacheContentType.NextLaunch);
            var nextLaunchDateUtc = nextLaunch.DateUtc ?? DateTime.MaxValue;
            var minutesToLaunch = (nextLaunchDateUtc - DateTime.Now.ToUniversalTime()).TotalMinutes;

            if (nextLaunch.DatePrecision != DatePrecision.Hour)
            {
                return;
            }

            if (_notified && minutesToLaunch > MinutesToLaunchToNotify)
            {
                _notified = false;
            }

            if (!_notified && minutesToLaunch >= 0 && minutesToLaunch <= MinutesToLaunchToNotify)
            {
                _notified = true;

                using (var databaseContext = new DatabaseContext())
                {
                    var usersToNotify = databaseContext.UserLaunchSubscriptions.Where(p => p.LaunchId == nextLaunch.FlightNumber).ToList();
                    var sentWithSuccess = 0;

                    var stopwatch = Stopwatch.StartNew();
                    foreach (var user in usersToNotify)
                    {
                        try
                        {
                            var guild = await Bot.Client.GetGuildAsync(ulong.Parse(user.GuildId));
                            var member = await guild.GetMemberAsync(ulong.Parse(user.UserId));

                            await SendLaunchNotificationToUserAsync(member, nextLaunch);
                            sentWithSuccess++;
                        }
                        catch (UnauthorizedException ex)
                        {
                            _logger.Warn($"No permissions to send user launch notification to [{user.UserId}] (from guild [{user.GuildId}])");
                            _logger.Warn($"JSON: {ex.JsonMessage}");
                        }
                        catch (NotFoundException ex)
                        {
                            _logger.Warn($"Can't send user launch notification, user [{user.UserId}] (from guild [{user.GuildId}]) not found");
                            _logger.Warn($"JSON: {ex.JsonMessage}");
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, $"Can't send user launch notification to [{user.UserId}] (from guild [{user.GuildId}])");
                        }
                    }

                    _logger.Info($"{minutesToLaunch:0.0} minutes to launch! {usersToNotify.Count} notifications sent " +
                                 $"({usersToNotify.Count - sentWithSuccess} errors) in {stopwatch.Elapsed.TotalSeconds:0.0} seconds");
                }
            }
        }

        private async Task SendLaunchNotificationToUserAsync(DiscordMember member, LaunchInfo nextLaunch)
        {
            var launchInfoEmbed = _launchInfoEmbedGenerator.Build(nextLaunch, null, false);
            await member.SendMessageAsync($"**{MinutesToLaunchToNotify} minutes to launch!**", false, launchInfoEmbed);

            if (nextLaunch.Links.Webcast != null)
            {
                await member.SendMessageAsync($"Watch launch at stream: {nextLaunch.Links.Webcast}");
            }

            await member.SendMessageAsync("*You received this message because we noticed that you subscribed this launch. Remember that " +
                                          "subscription is one-time and you have to do it again if you want to receive similar notification " +
                                          "about next launch in the future.*");
        }

        private async void Notifications_UpdateTimerOnElapsedAsync(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            try
            {
                await UpdateLaunchNotifications();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "General error occurred when trying to send user launch notifications");
            }
        }
    }
}
