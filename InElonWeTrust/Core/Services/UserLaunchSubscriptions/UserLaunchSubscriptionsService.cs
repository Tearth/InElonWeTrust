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

        private async Task AddUserSubscription(ulong messageId, ulong userId)
        {
            var fixedMessageId = messageId.ToString();
            var fixedUserId = userId.ToString();
            var nextLaunch = await _cacheService.Get<LaunchInfo>(CacheContentType.NextLaunch);

            using (var databaseContext = new DatabaseContext())
            {
                if (databaseContext.MessagesToSubscribe.Any(p => p.MessageId == fixedMessageId) &&
                    !databaseContext.UserLaunchSubscriptions.Any(p => p.UserId == fixedUserId))
                {
                    databaseContext.UserLaunchSubscriptions.Add(new UserLaunchSubscription(nextLaunch.FlightNumber.Value, fixedUserId));
                    databaseContext.SaveChanges();
                }
            }
        }

        private async Task RemoveUserSubscription(ulong messageId, ulong userId)
        {
            var fixedMessageId = messageId.ToString();
            var fixedUserId = userId.ToString();
            var nextLaunch = await _cacheService.Get<LaunchInfo>(CacheContentType.NextLaunch);

            using (var databaseContext = new DatabaseContext())
            {
                if (databaseContext.MessagesToSubscribe.Any(p => p.MessageId == fixedMessageId))
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
        }

        private async Task ClientOnMessageReactionAdded(MessageReactionAddEventArgs e)
        {
            if (!e.User.IsBot && e.Emoji.GetDiscordName() == SubscribeEmojiName)
            {
                await AddUserSubscription(e.Message.Id, e.User.Id);
                _logger.Info($"User {e.User.Username} from {e.Channel.Guild.Name} has been added to the launch subscription list.");
            }
        }

        private async Task ClientOnMessageReactionRemoved(MessageReactionRemoveEventArgs e)
        {
            if (!e.User.IsBot && e.Emoji.GetDiscordName() == SubscribeEmojiName)
            {
                await RemoveUserSubscription(e.Message.Id, e.User.Id);
                _logger.Info($"User {e.User.Username} from {e.Channel.Guild.Name} has been removed from the launch subscription list.");
            }
        }

        private async void Notifications_UpdateTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
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
                            var discordUser = await Bot.Client.GetUserAsync(ulong.Parse(user.UserId));
                            var userDm = await Bot.Client.CreateDmAsync(discordUser);

                            await userDm.SendMessageAsync($"**{MinutesToLaunchToNotify} minutes to launch!**", false, _launchInfoEmbedGenerator.Build(nextLaunch, false));

                            if (nextLaunch.Links.VideoLink != null)
                            {
                                await userDm.SendMessageAsync($"Watch launch at stream: {nextLaunch.Links.VideoLink}");
                            }

                            await userDm.SendMessageAsync("*You received this message because we noticed that you subscribed this launch. Remember that " +
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
    }
}
