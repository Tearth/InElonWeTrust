using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Database.Models;
using InElonWeTrust.Core.Services.Cache;
using InElonWeTrust.Core.Services.Pagination;
using Oddity;
using Oddity.API.Models.Launch;

namespace InElonWeTrust.Core.Services.UserLaunchSubscriptions
{
    public class UserLaunchSubscriptionsService
    {
        private CacheService _cacheService;

        public UserLaunchSubscriptionsService(CacheService cacheService)
        {
            _cacheService = cacheService;

            Bot.Client.MessageReactionAdded += ClientOnMessageReactionAdded;
            Bot.Client.MessageReactionRemoved += ClientOnMessageReactionRemoved;
        }

        private async void AddUserSubscription(ulong messageId, ulong userId)
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

        private async void RemoveUserSubscription(ulong messageId, ulong userId)
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
            AddUserSubscription(e.Message.Id, e.User.Id);
        }

        private async Task ClientOnMessageReactionRemoved(MessageReactionRemoveEventArgs e)
        {
            RemoveUserSubscription(e.Message.Id, e.User.Id);
        }
    }
}
