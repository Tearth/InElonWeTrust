using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private const int ItemsPerPage = 10;

        public async void InitPagination(DiscordMessage message, PaginationContentType contentType)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var paginatedMessage = new PaginatedMessage
                {
                    MessageID = message.Id.ToString(),
                    ContentType = contentType,
                    CurrentPage = 1
                };

                databaseContext.PaginatedMessages.Add(paginatedMessage);
                databaseContext.SaveChanges();
            }

            await message.CreateReactionAsync(DiscordEmoji.FromName(Bot.Client, FirstEmojiName));
            await message.CreateReactionAsync(DiscordEmoji.FromName(Bot.Client, LeftEmojiName));
            await message.CreateReactionAsync(DiscordEmoji.FromName(Bot.Client, RightEmojiName));
            await message.CreateReactionAsync(DiscordEmoji.FromName(Bot.Client, LastEmojiName));
        }

        public bool IsPaginationSet(DiscordMessage message, PaginationContentType contentType)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var messageIdString = message.Id.ToString();
                return databaseContext.PaginatedMessages.Any(p => p.MessageID == messageIdString && p.ContentType == contentType);
            }
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

        public void GoToNextPage(DiscordMessage message, int totalItemsCount)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var messageIdString = message.Id.ToString();
                var pagination = databaseContext.PaginatedMessages.First(p => p.MessageID == messageIdString);

                if (pagination.CurrentPage * ItemsPerPage < totalItemsCount)
                {
                    pagination.CurrentPage++;
                    databaseContext.SaveChanges();
                }
            }
        }

        public void GoToPreviousPage(DiscordMessage message, int totalItemsCount)
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

        public void GoToFirstPage(DiscordMessage message, int totalItemsCount)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var messageIdString = message.Id.ToString();
                var pagination = databaseContext.PaginatedMessages.First(p => p.MessageID == messageIdString);

                pagination.CurrentPage = 1;
                databaseContext.SaveChanges();
            }
        }

        public void GoToLastPage(DiscordMessage message, int totalItemsCount)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var messageIdString = message.Id.ToString();
                var pagination = databaseContext.PaginatedMessages.First(p => p.MessageID == messageIdString);

                pagination.CurrentPage = totalItemsCount / ItemsPerPage + 1;
                databaseContext.SaveChanges();
            }
        }
    }
}