using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Database.Models;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace InElonWeTrust.Core.Services.Subscriptions
{
    public class SubscriptionsService
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public async Task<bool> AddSubscriptionAsync(ulong guildId, ulong channelId, SubscriptionType type)
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
                    GuildId = guildId.ToString(),
                    ChannelId = channelId.ToString(),
                    SubscriptionType = type
                };

                await databaseContext.SubscribedChannels.AddAsync(subscribedChannel);
                await databaseContext.SaveChangesAsync();

                _logger.Info($"Subscription for {type} added (guild [{guildId}], channel [{channelId}])");
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

                _logger.Info($"Subscription for {type} removed (channel [{channelId}])");
            }

            return true;
        }

        public async Task AddAllSubscriptionsAsync(ulong guildId, ulong channelId)
        {
            foreach (var subscriptionType in (SubscriptionType[])Enum.GetValues(typeof(SubscriptionType)))
            {
                await AddSubscriptionAsync(guildId, channelId, subscriptionType);
            }
        }

        public async Task RemoveAllSubscriptionsFromGuildAsync(ulong guildId)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var fixedGuildId = guildId.ToString();
                var channelIds = databaseContext.SubscribedChannels.Where(p => p.GuildId == fixedGuildId).GroupBy(p => p.ChannelId).Select(p => p.Key).ToList();

                foreach (var channelId in channelIds)
                {
                    foreach (var subscriptionType in (SubscriptionType[])Enum.GetValues(typeof(SubscriptionType)))
                    {
                        await RemoveSubscriptionAsync(ulong.Parse(channelId), subscriptionType);
                    }
                }
            }
        }

        public async Task RemoveAllSubscriptionsFromChannelAsync(ulong channelId)
        {
            foreach (var subscriptionType in (SubscriptionType[])Enum.GetValues(typeof(SubscriptionType)))
            {
                await RemoveSubscriptionAsync(channelId, subscriptionType);
            }
        }

        public List<SubscribedChannel> GetSubscribedChannels(SubscriptionType type)
        {
            using (var databaseContext = new DatabaseContext())
            {
                return databaseContext.SubscribedChannels
                    .Where(p => p.SubscriptionType == type)
                    .ToList();
            }
        }

        public async Task<bool> IsChannelSubscribed(ulong channelId, SubscriptionType type)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var fixedChannelId = channelId.ToString();
                return await databaseContext.SubscribedChannels.AnyAsync(p => p.ChannelId == fixedChannelId && p.SubscriptionType == type);
            }
        }

        public async Task<SubscriptionStatus> GetSubscriptionStatusForChannel(ulong channelId)
        {
            return new SubscriptionStatus
            {
                ElonTwitter = await IsChannelSubscribed(channelId, SubscriptionType.ElonTwitter),
                SpaceXTwitter = await IsChannelSubscribed(channelId, SubscriptionType.SpaceXTwitter),
                SpaceXFleetTwitter = await IsChannelSubscribed(channelId, SubscriptionType.SpaceXFleetTwitter),
                Flickr = await IsChannelSubscribed(channelId, SubscriptionType.Flickr),
                Launches = await IsChannelSubscribed(channelId, SubscriptionType.NextLaunch),
                Reddit = await IsChannelSubscribed(channelId, SubscriptionType.Reddit)
            };
        }
    }
}
