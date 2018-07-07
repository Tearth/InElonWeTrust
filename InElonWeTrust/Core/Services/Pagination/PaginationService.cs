using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Database.Models;

namespace InElonWeTrust.Core.Services.Pagination
{
    public class PaginationService
    {
        private const string FirstEmojiName = ":track_previous:";
        private const string LeftEmojiName = ":arrow_left:";
        private const string RightEmojiName = ":arrow_right:";
        private const string LastEmojiName = ":track_next:";
        private const int ItemsPerPage = 15;

        public async Task InitPagination(DiscordMessage message, PaginationContentType contentType)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var paginatedMessage = new PaginatedMessage
                {
                    MessageID = message.Id.ToString(),
                    ContentType = contentType,
                    CurrentPage = 1
                };

                await databaseContext.PaginatedMessages.AddAsync(paginatedMessage);
                await databaseContext.SaveChangesAsync();
            }

            await message.CreateReactionAsync(DiscordEmoji.FromName(Bot.Client, FirstEmojiName));
            await message.CreateReactionAsync(DiscordEmoji.FromName(Bot.Client, LeftEmojiName));
            await message.CreateReactionAsync(DiscordEmoji.FromName(Bot.Client, RightEmojiName));
            await message.CreateReactionAsync(DiscordEmoji.FromName(Bot.Client, LastEmojiName));
        }

        public bool IsPaginationSet(DiscordMessage message)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var messageIdString = message.Id.ToString();
                return databaseContext.PaginatedMessages.Any(p => p.MessageID == messageIdString);
            }
        }

        public List<T> GetItemsToDisplay<T>(List<T> items, int page)
        {
            return items.Skip((page - 1) * ItemsPerPage).Take(ItemsPerPage).ToList();
        }

        public int GetPagesCount(int totalItemsCount)
        {
            return totalItemsCount / ItemsPerPage + 1;
        }

        public int GetCurrentPage(DiscordMessage message)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var messageIdString = message.Id.ToString();
                var pagination = databaseContext.PaginatedMessages.First(p => p.MessageID == messageIdString);

                return pagination.CurrentPage;
            }
        }

        public PaginationContentType GetContentTypeForMessage(DiscordMessage message)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var messageIdString = message.Id.ToString();
                var pagination = databaseContext.PaginatedMessages.First(p => p.MessageID == messageIdString);

                return pagination.ContentType;
            }
        }

        public string GetPaginationFooter(int currentPage, int maxPagesCount)
        {
            return $"page {currentPage} from {maxPagesCount}";
        }

        public void DoAction(DiscordMessage message, DiscordEmoji clickedEmoji, int totalItemsCount)
        {
            switch (clickedEmoji.GetDiscordName())
            {
                case RightEmojiName: GoToNextPage(message, totalItemsCount); break;
                case LeftEmojiName: GoToPreviousPage(message); break;
                case FirstEmojiName: GoToFirstPage(message); break;
                case LastEmojiName: GoToLastPage(message, totalItemsCount); break;
            }
        }

        private void GoToNextPage(DiscordMessage message, int totalItemsCount)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var messageIdString = message.Id.ToString();
                var pagination = databaseContext.PaginatedMessages.First(p => p.MessageID == messageIdString);

                if (pagination.CurrentPage < GetPagesCount(totalItemsCount))
                {
                    pagination.CurrentPage++;
                    databaseContext.SaveChanges();
                }
            }
        }

        private void GoToPreviousPage(DiscordMessage message)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var messageIdString = message.Id.ToString();
                var pagination = databaseContext.PaginatedMessages.First(p => p.MessageID == messageIdString);

                if (pagination.CurrentPage > 1)
                {
                    pagination.CurrentPage--;
                    databaseContext.SaveChanges();
                }
            }
        }

        private void GoToFirstPage(DiscordMessage message)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var messageIdString = message.Id.ToString();
                var pagination = databaseContext.PaginatedMessages.First(p => p.MessageID == messageIdString);

                pagination.CurrentPage = 1;
                databaseContext.SaveChanges();
            }
        }

        private void GoToLastPage(DiscordMessage message, int totalItemsCount)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var messageIdString = message.Id.ToString();
                var pagination = databaseContext.PaginatedMessages.First(p => p.MessageID == messageIdString);

                pagination.CurrentPage = GetPagesCount(totalItemsCount);
                databaseContext.SaveChanges();
            }
        }
    }
}