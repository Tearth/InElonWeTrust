﻿using InElonWeTrust.Core.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace InElonWeTrust.Core.Database
{
    public class DatabaseContext : DbContext
    {
        public DbSet<CachedTweet> CachedTweets { get; set; }
        public DbSet<CachedFlickrPhoto> CachedFlickrPhotos { get; set; }
        public DbSet<CachedRedditTopic> CachedRedditTopics { get; set; }
        public DbSet<PaginatedMessage> PaginatedMessages { get; set; }
        public DbSet<SubscribedChannel> SubscribedChannels { get; set; }
        public DbSet<MessageToSubscribe> MessagesToSubscribe { get; set; }
        public DbSet<UserLaunchSubscription> UserLaunchSubscriptions { get; set; }
        public DbSet<Quote> Quotes { get; set; }

        public DatabaseContext() : base(GetOptions("Data Source=Database.sqlite"))
        {

        }

        private static DbContextOptions GetOptions(string connectionString)
        {
            return SqliteDbContextOptionsBuilderExtensions.UseSqlite(new DbContextOptionsBuilder(), connectionString).Options;
        }
    }
}
