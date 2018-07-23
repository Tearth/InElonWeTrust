using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Quotes;

namespace InElonWeTrust.Core.Commands
{
    [Commands(GroupType.Miscellaneous)]
    public class QuotesCommands
    {
        private readonly QuotesService _quotesService;
        private readonly QuoteEmbedGenerator _quoteEmbedGenerator;

        public QuotesCommands(QuotesService quotesService, QuoteEmbedGenerator quoteEmbedGenerator)
        {
            _quotesService = quotesService;
            _quoteEmbedGenerator = quoteEmbedGenerator;
        }

        [Command("RandomElonQuote")]
        [Aliases("RandomQuote", "Quote", "q")]
        [Description("Get random Elon quote.")]
        public async Task GetElonQuote(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var quote = await _quotesService.GetRandomQuoteAsync();
            var embed = _quoteEmbedGenerator.Build(quote);

            await ctx.RespondAsync(embed: embed);
        }
    }
}
