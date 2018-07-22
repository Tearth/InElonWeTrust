using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Services.Cache;
using Oddity;

namespace InElonWeTrust.Core.Commands
{
    [Commands(GroupType.Miscellaneous)]
    public class CompanyHistoryEventCommand
    {
        private readonly OddityCore _oddity;
        private readonly CacheService _cacheService;
        private readonly CompanyHistoryEventEmbedGenerator _companyHistoryEventEmbedGenerator;

        public CompanyHistoryEventCommand(OddityCore oddity, CacheService cacheService, CompanyHistoryEventEmbedGenerator companyHistoryEventEmbedGenerator)
        {
            _oddity = oddity;
            _cacheService = cacheService;
            _companyHistoryEventEmbedGenerator = companyHistoryEventEmbedGenerator;
        }

        [Command("GetEvent")]
        [Aliases("Event", "e")]
        [Description("Get information about event with specified id (e!CompanyHistory).")]
        public async Task GetEvent(CommandContext ctx, int id)
        {
            await ctx.TriggerTypingAsync();

            var history = (await _oddity.Company.GetHistory().ExecuteAsync()).OrderBy(p => p.EventDate.Value).ToList();
            if (id <= 0 || id > history.Count)
            {
                var errorEmbed = _companyHistoryEventEmbedGenerator.BuildError();
                await ctx.RespondAsync("", false, errorEmbed);
            }
            else
            {
                var embed = _companyHistoryEventEmbedGenerator.Build(history[id - 1]);
                await ctx.RespondAsync("", false, embed);
            }
        }
    }
}
