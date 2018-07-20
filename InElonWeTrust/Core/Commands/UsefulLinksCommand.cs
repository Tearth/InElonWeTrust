using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.UsefulLinks;

namespace InElonWeTrust.Core.Commands
{
    [Commands(":question:", "Misc", "Other strange commands")]
    public class UsefulLinksCommand
    {
        private UsefulLinksService _userfulLunksService;

        public UsefulLinksCommand(UsefulLinksService userfulLunksService)
        {
            _userfulLunksService = userfulLunksService;
        }

        [Command("Links")]
        [Description("Get list of useful links.")]
        public async Task Uptime(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var links = _userfulLunksService.GetUsefulLinks();
            var embedBuilder = new DiscordEmbedBuilder
            {
                Title = "Useful links",
                Color = new DiscordColor(Constants.EmbedColor)
            };

            var firstColumnContentBuilder = new StringBuilder();
            var secondColumnContentBuilder = new StringBuilder();

            var firstColumn = links.GetRange(0, links.Count / 2);
            var secondColumn = links.GetRange(links.Count / 2, links.Count - (links.Count / 2));

            foreach (var link in firstColumn)
            {
                firstColumnContentBuilder.Append($"[{link.Name}]({link.Link})\r\n");
            }

            foreach (var link in secondColumn)
            {
                secondColumnContentBuilder.Append($"[{link.Name}]({link.Link})\r\n");
            }

            embedBuilder.AddField("\u200b", firstColumnContentBuilder.ToString(), true);
            embedBuilder.AddField("\u200b", secondColumnContentBuilder.ToString(), true);

            await ctx.RespondAsync("", false, embedBuilder);
        }
    }
}
