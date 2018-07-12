using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace InElonWeTrust.Core.Services.Subscriptions
{
    public class SubscriptionsService
    {
        public async Task<bool> AddSubscriptionAsync(ulong channelId, SubscriptionType type)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var fixedChannelId = channelId.ToString();
                if (await databaseContext.SubscribedChannels.AnyAsync(p => p.ChannelId == fixedChannelId && p.SubscriptionType == type))
                {
                    return false;
                }

                var subscribedChannel = new SubscribedChannel
                {
                    ChannelId = channelId.ToString(),
                    SubscriptionType = type
                };

                await databaseContext.SubscribedChannels.AddAsync(subscribedChannel);
                await databaseContext.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> RemoveSubscriptionAsync(ulong channelId, SubscriptionType type)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var fixedChannelId = channelId.ToString();
                var subscribedChannel = await databaseContext.SubscribedChannels.FirstOrDefaultAsync(p => p.ChannelId == fixedChannelId && p.SubscriptionType == type);

                if (subscribedChannel == null)
                {
                    return false;
                }

                databaseContext.SubscribedChannels.Remove(subscribedChannel);
                await databaseContext.SaveChangesAsync();
            }

            return true;
        }

        public List<ulong> GetSubscribedChannels(SubscriptionType type)
        {
            using (var databaseContext = new DatabaseContext())
            {
                return databaseContext.SubscribedChannels
                    .Where(p => p.SubscriptionType == type)
                    .ToList()
                    .Select(p => ulong.Parse(p.ChannelId))
                    .ToList();
            }
        }

        public async Task<bool> IsChannelSubscribed(ulong channelId, SubscriptionType type)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var fixedChannelId = channelId.ToString();
                return await databaseContext.SubscribedChannels.AnyAsync(p => p.ChannelId == fixedChannelId);
            }
        }
    }
}
