using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Database.Models;
using InElonWeTrust.Core.Services.Cache;
using Microsoft.EntityFrameworkCore;
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

        public async Task InitPaginationAsync(DiscordMessage message, CacheContentType contentType, string parameter)
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

        public async Task<bool> IsPaginationSetAsync(DiscordMessage message)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var messageIdString = message.Id.ToString();
                return await databaseContext.PaginatedMessages.AnyAsync(p => p.MessageId == messageIdString);
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

        public async Task<PaginatedMessage> GetPaginationDataForMessageAsync(DiscordMessage message)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var messageIdString = message.Id.ToString();
                return await databaseContext.PaginatedMessages.FirstAsync(p => p.MessageId == messageIdString);
            }
        }

        public string GetPaginationFooter(int currentPage, int maxPagesCount)
        {
            return $"page {currentPage} from {maxPagesCount}";
        }

        public async Task<bool> DoActionAsync(DiscordMessage message, DiscordEmoji clickedEmoji, int totalItemsCount)
        {
            switch (clickedEmoji.GetDiscordName())
            {
                case RightEmojiName: return await GoToNextPageAsync(message, totalItemsCount);
                case LeftEmojiName: return await GoToPreviousPageAsync(message);
                case FirstEmojiName: return await GoToFirstPageAsync(message);
                case LastEmojiName: return await GoToLastPageAsync(message, totalItemsCount);
            }

            return false;
        }

        public async Task<bool> DeleteReactionAsync(DiscordMessage message, DiscordUser reactionUser, DiscordEmoji emoji)
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

        private async Task<bool> GoToNextPageAsync(DiscordMessage message, int totalItemsCount)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var messageIdString = message.Id.ToString();
                var pagination = await databaseContext.PaginatedMessages.FirstAsync(p => p.MessageId == messageIdString);

                if (pagination.CurrentPage < GetPagesCount(totalItemsCount))
                {
                    pagination.CurrentPage++;
                    await databaseContext.SaveChangesAsync();

                    return true;
                }
            }

            return false;
        }

        private async Task<bool> GoToPreviousPageAsync(DiscordMessage message)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var messageIdString = message.Id.ToString();
                var pagination = await databaseContext.PaginatedMessages.FirstAsync(p => p.MessageId == messageIdString);

                if (pagination.CurrentPage > 1)
                {
                    pagination.CurrentPage--;
                    await databaseContext.SaveChangesAsync();

                    return true;
                }
            }

            return false;
        }

        private async Task<bool> GoToFirstPageAsync(DiscordMessage message)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var messageIdString = message.Id.ToString();
                var pagination = await databaseContext.PaginatedMessages.FirstAsync(p => p.MessageId == messageIdString);

                pagination.CurrentPage = 1;
                await databaseContext.SaveChangesAsync();

                return true;
            }
        }

        private async Task<bool> GoToLastPageAsync(DiscordMessage message, int totalItemsCount)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var messageIdString = message.Id.ToString();
                var pagination = await databaseContext.PaginatedMessages.FirstAsync(p => p.MessageId == messageIdString);

                pagination.CurrentPage = GetPagesCount(totalItemsCount);
                await databaseContext.SaveChangesAsync();

                return true;
            }
        }
    }
}