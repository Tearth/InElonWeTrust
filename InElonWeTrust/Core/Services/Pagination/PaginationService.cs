using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Database.Models;
using InElonWeTrust.Core.Services.Cache;
using NLog;

namespace InElonWeTrust.Core.Services.Pagination
{
    public class PaginationService
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private const string FirstEmojiName = ":previous_track:";
        private const string LeftEmojiName = ":arrow_left:";
        private const string RightEmojiName = ":arrow_right:";
        private const string LastEmojiName = ":next_track:";
        public const int ItemsPerPage = 15;

        public async Task InitPagination(DiscordMessage message, CacheContentType contentType, string parameter)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var paginatedMessage = new PaginatedMessage
                {
                    GuildId = message.Channel.GuildId.ToString(),
                    MessageId = message.Id.ToString(),
                    ContentType = contentType,
                    Parameter = parameter,
                    CurrentPage = 1
                };

                await databaseContext.PaginatedMessages.AddAsync(paginatedMessage);
                await databaseContext.SaveChangesAsync();
            }

            await message.CreateReactionAsync(DiscordEmoji.FromName(Bot.Client, FirstEmojiName));
            await message.CreateReactionAsync(DiscordEmoji.FromName(Bot.Client, LeftEmojiName));
            await message.CreateReactionAsync(DiscordEmoji.FromName(Bot.Client, RightEmojiName));
            await message.CreateReactionAsync(DiscordEmoji.FromName(Bot.Client, LastEmojiName));

            _logger.Info($"New pagination for {contentType} added");
        }

        public bool IsPaginationSet(DiscordMessage message)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var messageIdString = message.Id.ToString();
                return databaseContext.PaginatedMessages.Any(p => p.MessageId == messageIdString);
            }
        }

        public List<T> GetItemsToDisplay<T>(List<T> items, int page)
        {
            return items.Skip((page - 1) * ItemsPerPage).Take(ItemsPerPage).ToList();
        }

        public int GetPagesCount(int totalItemsCount)
        {
            return (int)Math.Ceiling((double)totalItemsCount / ItemsPerPage);
        }

        public PaginatedMessage GetPaginationDataForMessage(DiscordMessage message)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var messageIdString = message.Id.ToString();
                return databaseContext.PaginatedMessages.First(p => p.MessageId == messageIdString);
            }
        }

        public string GetPaginationFooter(int currentPage, int maxPagesCount)
        {
            return $"page {currentPage} from {maxPagesCount}";
        }

        public bool DoAction(DiscordMessage message, DiscordEmoji clickedEmoji, int totalItemsCount)
        {
            switch (clickedEmoji.GetDiscordName())
            {
                case RightEmojiName: return GoToNextPage(message, totalItemsCount);
                case LeftEmojiName: return GoToPreviousPage(message);
                case FirstEmojiName: return GoToFirstPage(message);
                case LastEmojiName: return GoToLastPage(message, totalItemsCount);
            }

            return false;
        }

        public async Task<bool> DeleteReaction(DiscordMessage message, DiscordUser reactionUser, DiscordEmoji emoji)
        {
            try
            {
                await message.DeleteReactionAsync(emoji, reactionUser);
            }
            catch (UnauthorizedException)
            {
                var messageContent = message.Content ?? (await message.Channel.GetMessageAsync(message.Id)).Content;

                if (messageContent.EndsWith("```", StringComparison.InvariantCultureIgnoreCase))
                {
                    messageContent += "\r\n";
                    messageContent += "*It seems that I have not enough permissions to do pagination properly. Please check " +
                                      "bot/channel permissions and be sure that I have ability to manage messages.*";
                }

                _logger.Warn("Can't do pagination due to permissions.");
                await message.ModifyAsync(messageContent);

                return false;
            }

            return true;
        }

        private bool GoToNextPage(DiscordMessage message, int totalItemsCount)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var messageIdString = message.Id.ToString();
                var pagination = databaseContext.PaginatedMessages.First(p => p.MessageId == messageIdString);

                if (pagination.CurrentPage < GetPagesCount(totalItemsCount))
                {
                    pagination.CurrentPage++;
                    databaseContext.SaveChanges();

                    return true;
                }
            }

            return false;
        }

        private bool GoToPreviousPage(DiscordMessage message)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var messageIdString = message.Id.ToString();
                var pagination = databaseContext.PaginatedMessages.First(p => p.MessageId == messageIdString);

                if (pagination.CurrentPage > 1)
                {
                    pagination.CurrentPage--;
                    databaseContext.SaveChanges();

                    return true;
                }
            }

            return false;
        }

        private bool GoToFirstPage(DiscordMessage message)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var messageIdString = message.Id.ToString();
                var pagination = databaseContext.PaginatedMessages.First(p => p.MessageId == messageIdString);

                pagination.CurrentPage = 1;
                databaseContext.SaveChanges();

                return true;
            }
        }

        private bool GoToLastPage(DiscordMessage message, int totalItemsCount)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var messageIdString = message.Id.ToString();
                var pagination = databaseContext.PaginatedMessages.First(p => p.MessageId == messageIdString);

                pagination.CurrentPage = GetPagesCount(totalItemsCount);
                databaseContext.SaveChanges();

                return true;
            }
        }
    }
}