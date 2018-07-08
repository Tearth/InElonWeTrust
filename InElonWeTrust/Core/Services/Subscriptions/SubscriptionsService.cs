using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Database.Models;
using Microsoft.EntityFrameworkCore.Internal;

namespace InElonWeTrust.Core.Services.Subscriptions
{
    public class SubscriptionsService
    {
        public async Task<bool> AddSubscription(long channelID, SubscriptionType type)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var fixedChannelId = channelID.ToString();
                if (databaseContext.SubscribedChannels.Any(p => p.ChannelID == fixedChannelId && p.SubscriptionType == type))
                {
                    return false;
                }

                var subscribedChannel = new SubscribedChannel
                {
                    ChannelID = channelID.ToString(),
                    SubscriptionType = type
                };

                await databaseContext.SubscribedChannels.AddAsync(subscribedChannel);
                await databaseContext.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> RemoveSubscription(long channelID, SubscriptionType type)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var fixedChannelId = channelID.ToString();
                var subscribedChannel = databaseContext.SubscribedChannels.FirstOrDefault(p => p.ChannelID == fixedChannelId && p.SubscriptionType == type);

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
                    .Select(p => ulong.Parse(p.ChannelID))
                    .ToList();
            }
        }
    }
}
