﻿using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Changelog;

namespace InElonWeTrust.Core.Commands
{
    [Commands(GroupType.Miscellaneous)]
    public class ChangelogCommand
    {
        private readonly ChangelogService _changelogService;

        public ChangelogCommand(ChangelogService changelogService)
        {
            _changelogService = changelogService;
        }

        [Command("Changelog")]
        [Description("Get bot changelog.")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor)
            };

            var changelog = await _changelogService.GetChangelog();
            embed.AddField("Changelog", changelog);

            await ctx.RespondAsync("", false, embed);
        }
    }
}
