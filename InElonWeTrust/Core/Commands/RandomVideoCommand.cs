﻿using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.Database;

namespace InElonWeTrust.Core.Commands
{
    [Commands(GroupType.Miscellaneous)]
    public class RandomVideoCommand
    {
        [Command("RandomVideo")]
        [Aliases("Video", "rv")]
        [Description("Get random video related with SpaceX.")]
        public async Task RandomVideo(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            using (var databaseContext = new DatabaseContext())
            {
                var video = databaseContext.VideoLinks.OrderBy(p => Guid.NewGuid()).First();
                await ctx.RespondAsync(video.Link);
            }
        }
    }
}
