using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using DSharpPlus.EventArgs;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Database.Models;
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Services.Cache;
using NLog;
using Oddity.API.Models.Launch;

namespace InElonWeTrust.Core.Services.UserLaunchSubscriptions
{
    public class UserLaunchSubscriptionsService
    {
        private readonly CacheService _cacheService;
        private readonly LaunchInfoEmbedGenerator _launchInfoEmbedGenerator;
        private readonly Timer _notificationsUpdateTimer;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private bool _notified;

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
            _notificationsUpdateTimer.Elapsed += Notifications_UpdateTimerOnElapsed;
            _notificationsUpdateTimer.Start();
        }

        private async Task AddUserSubscription(ulong userId, ulong guildId)
        {
            var fixedUserId = userId.ToString();
            var fixedGuildId = guildId.ToString();

            var nextLaunch = await _cacheService.Get<LaunchInfo>(CacheContentType.NextLaunch);

            using (var databaseContext = new DatabaseContext())
            {
                if (!databaseContext.UserLaunchSubscriptions.Any(p => p.UserId == fixedUserId && p.LaunchId == nextLaunch.FlightNumber))
                {
                    databaseContext.UserLaunchSubscriptions.Add(new UserLaunchSubscription(nextLaunch.FlightNumber.Value, fixedGuildId, fixedUserId));
                    databaseContext.SaveChanges();
                }
            }
        }

        private async Task RemoveUserSubscription(ulong userId, ulong guildId)
        {
            var fixedUserId = userId.ToString();
            var fixedGuildId = guildId.ToString();

            var nextLaunch = await _cacheService.Get<LaunchInfo>(CacheContentType.NextLaunch);

            using (var databaseContext = new DatabaseContext())
            {
                var userSubscription = databaseContext.UserLaunchSubscriptions
                    .FirstOrDefault(p => p.UserId == fixedUserId && p.LaunchId == nextLaunch.FlightNumber);

                if (userSubscription != null)
                {
                    databaseContext.UserLaunchSubscriptions.Remove(userSubscription);
                    databaseContext.SaveChanges();
                }
            }
        }

        private bool IsMessageSubscribable(ulong messageId)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var fixedMessageId = messageId.ToString();
                return databaseContext.MessagesToSubscribe.Any(p => p.MessageId == fixedMessageId);
            }
        }

        private async Task ClientOnMessageReactionAdded(MessageReactionAddEventArgs e)
        {
            if (!e.User.IsBot && e.Emoji.GetDiscordName() == SubscribeEmojiName && IsMessageSubscribable(e.Message.Id))
            {
                await AddUserSubscription(e.User.Id, e.Channel.GuildId);
                _logger.Info($"User {e.User.Username} from {e.Channel.Guild.Name} has been added to the launch subscription list.");
            }
        }

        private async Task ClientOnMessageReactionRemoved(MessageReactionRemoveEventArgs e)
        {
            if (!e.User.IsBot && e.Emoji.GetDiscordName() == SubscribeEmojiName && IsMessageSubscribable(e.Message.Id))
            {
                await RemoveUserSubscription(e.User.Id, e.Channel.GuildId);
                _logger.Info($"User {e.User.Username} from {e.Channel.Guild.Name} has been removed from the launch subscription list.");
            }
        }

        private async Task UpdateLaunchNotifications()
        {
            var nextLaunch = await _cacheService.Get<LaunchInfo>(CacheContentType.NextLaunch);
            var minutesToLaunch = (nextLaunch.LaunchDateUtc.Value - DateTime.Now.ToUniversalTime()).TotalMinutes;

            if (_notified && minutesToLaunch > MinutesToLaunchToNotify)
            {
                _notified = false;
            }

            if (!_notified && minutesToLaunch <= MinutesToLaunchToNotify)
            {
                using (var databaseContext = new DatabaseContext())
                {
                    var usersToNotify = databaseContext.UserLaunchSubscriptions.Where(p => p.LaunchId == nextLaunch.FlightNumber).ToList();
                    foreach (var user in usersToNotify)
                    {
                        try
                        {
                            var guild = await Bot.Client.GetGuildAsync(ulong.Parse(user.GuildId));
                            var member = await guild.GetMemberAsync(ulong.Parse(user.UserId));

                            await member.SendMessageAsync($"**{MinutesToLaunchToNotify} minutes to launch!**", false, _launchInfoEmbedGenerator.Build(nextLaunch, false));

                            if (nextLaunch.Links.VideoLink != null)
                            {
                                await member.SendMessageAsync($"Watch launch at stream: {nextLaunch.Links.VideoLink}");
                            }

                            await member.SendMessageAsync("*You received this message because we noticed that you subscribed this launch. Remember that " +
                                                          "subscription is one-time and you have to do it again if you want to receive similar notification " +
                                                          "about next launch in the future.*");
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, $"Can't send launch notification to the uesr with id {user.UserId}");
                        }
                    }

                    _logger.Info($"{minutesToLaunch} to launch! {usersToNotify.Count} sent");
                }

                _notified = true;
            }
        }

        private async void Notifications_UpdateTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            try
            {
                await UpdateLaunchNotifications();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Can't update user launch notifications");
            }
        }
    }
}
