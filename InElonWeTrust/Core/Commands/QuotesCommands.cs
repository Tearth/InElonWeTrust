using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Services.Quotes;

namespace InElonWeTrust.Core.Commands
{
    [CommandsGroup(GroupType.Miscellaneous)]
    public class QuotesCommands : BaseCommandModule
    {
        private readonly QuotesService _quotesService;
        private readonly QuoteEmbedGenerator _quoteEmbedGenerator;

        public QuotesCommands(QuotesService quotesService, QuoteEmbedGenerator quoteEmbedGenerator)
        {
            _quotesService = quotesService;
            _quoteEmbedGenerator = quoteEmbedGenerator;
        }

        [Command("RandomElonQuote"), Aliases("RandomQuote", "ElonQuote", "Quote")]
        [Description("Get a random Elon quote.")]
        public async Task GetElonQuoteAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var quote = await _quotesService.GetRandomQuoteAsync();
            var embed = _quoteEmbedGenerator.Build(quote);

            await ctx.RespondAsync(embed: embed);
        }
    }
}
