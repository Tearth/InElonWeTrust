using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Changelog;

namespace InElonWeTrust.Core.Commands
{
    [Commands(":question:", "Misc", "Other strange commands")]
    public class ChangelogCommand
    {
        private ChangelogService _changelogService;

        public ChangelogCommand()
        {
            _changelogService = new ChangelogService();
        }

        [Command("Changelog")]
        [Description("Get bot changelog.")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor),
                ThumbnailUrl = Constants.ThumbnailImage
            };

            var changelog = await _changelogService.GetChangelog();
            embed.AddField("Changelog", changelog);

            await ctx.RespondAsync("", false, embed);
        }
    }
}
