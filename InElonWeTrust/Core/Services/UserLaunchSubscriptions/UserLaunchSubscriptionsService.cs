using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using DSharpPlus.EventArgs;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Database.Models;
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Services.Cache;
using InElonWeTrust.Core.Services.Pagination;
using Oddity;
using Oddity.API.Models.Launch;

namespace InElonWeTrust.Core.Services.UserLaunchSubscriptions
{
    public class UserLaunchSubscriptionsService
    {
        private CacheService _cacheService;
        private LaunchInfoEmbedGenerator _launchInfoEmbedGenerator;
        private Timer _notificationsUpdateTimer;

        private bool _notified;

        private const int IntervalMinutes = 1;
        private const int MinutesToLaunchToNotify = 10;

        public UserLaunchSubscriptionsService(CacheService cacheService, LaunchInfoEmbedGenerator launchInfoEmbedGenerator)
        {
            _cacheService = cacheService;
            _launchInfoEmbedGenerator = launchInfoEmbedGenerator;

            Bot.Client.MessageReactionAdded += ClientOnMessageReactionAdded;
            Bot.Client.MessageReactionRemoved += ClientOnMessageReactionRemoved;

            _notificationsUpdateTimer = new Timer(IntervalMinutes * 60 * 1000);
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
            if (!e.User.IsBot)
            {
                await AddUserSubscription(e.Message.Id, e.User.Id);
            }
        }

        private async Task ClientOnMessageReactionRemoved(MessageReactionRemoveEventArgs e)
        {
            if (!e.User.IsBot)
            {
                await RemoveUserSubscription(e.Message.Id, e.User.Id);
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
                        var discordUser = await Bot.Client.GetUserAsync(ulong.Parse(user.UserId));
                        var userDm = await Bot.Client.CreateDmAsync(discordUser);

                        await userDm.SendMessageAsync($"**{MinutesToLaunchToNotify} minutes to launch!**", false, await _launchInfoEmbedGenerator.Build(nextLaunch, false));

                        if (nextLaunch.Links.VideoLink != null)
                        {
                            await userDm.SendMessageAsync($"Watch launch at stream: {nextLaunch.Links.VideoLink}");
                        }

                        await userDm.SendMessageAsync("*You received this message because we noticed that you subscribed this launch. Remember that " +
                                                      "subscription is one-time and you have to do it again if you want to receive similar notification " +
                                                      "about next launch in the future.*");
                    }
                }

                _notified = true;
            }
        }
    }
}
