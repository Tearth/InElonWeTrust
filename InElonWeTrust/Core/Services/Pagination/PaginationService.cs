using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus.Entities;

namespace InElonWeTrust.Core.Services.Pagination
{
    public class PaginationService
    {
        private const string FirstEmojiName = ":track_previous:";
        private const string LeftEmojiName = ":arrow_left:";
        private const string RightEmojiName = ":arrow_right:";
        private const string LastEmojiName = ":track_next:";

        public async void InitPagination(DiscordMessage message)
        {
            await message.CreateReactionAsync(DiscordEmoji.FromName(Bot.Client, FirstEmojiName));
            await message.CreateReactionAsync(DiscordEmoji.FromName(Bot.Client, LeftEmojiName));
            await message.CreateReactionAsync(DiscordEmoji.FromName(Bot.Client, RightEmojiName));
            await message.CreateReactionAsync(DiscordEmoji.FromName(Bot.Client, LastEmojiName));
        }
    }
}
