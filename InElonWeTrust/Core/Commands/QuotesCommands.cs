using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Database.Models;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Quotes;

namespace InElonWeTrust.Core.Commands
{
    [Commands("Misc")]
    public class QuotesCommands
    {
        private QuotesService _quotesService;

        public QuotesCommands()
        {
            _quotesService = new QuotesService();
        }

        [Command("getelonquote")]
        [Aliases("elonquote", "quote", "q")]
        [Description("Get random Elon quote.")]
        public async Task GetElonQuote(CommandContext ctx)
        {
            var quote = await _quotesService.GetRandomQuoteAsync();
            await DisplayQuote(ctx, quote);
        }

        private async Task DisplayQuote(CommandContext ctx, string quote)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor),
            };

            embed.AddField("°", $"*{quote}*\r\n~Elon Musk");

            await ctx.RespondAsync("", false, embed);
        }
    }
}
