using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Services.Cache;
using Oddity;
using Oddity.API.Models.Company;

namespace InElonWeTrust.Core.Commands
{
    [Commands(GroupType.Miscellaneous)]
    public class CompanyHistoryEventCommand : BaseCommandModule
    {
        private readonly CacheService _cacheService;
        private readonly CompanyHistoryEventEmbedGenerator _companyHistoryEventEmbedGenerator;

        public CompanyHistoryEventCommand(OddityCore oddity, CacheService cacheService, CompanyHistoryEventEmbedGenerator companyHistoryEventEmbedGenerator)
        {
            _cacheService = cacheService;
            _companyHistoryEventEmbedGenerator = companyHistoryEventEmbedGenerator;

            _cacheService.RegisterDataProvider(CacheContentType.CompanyHistory, async p => await oddity.Company.GetHistory().ExecuteAsync());
        }

        [Command("GetEvent")]
        [Aliases("Event", "e")]
        [Description("Get information about event with the specified id (which can be obtained by `e!CompanyHistory`).")]
        public async Task GetEvent(CommandContext ctx, [Description("Event id which can be obtained by `e!CompanyHistory`")] int id)
        {
            await ctx.TriggerTypingAsync();

            var history = await _cacheService.Get<List<HistoryEvent>>(CacheContentType.CompanyHistory);
            var sortedHistory = history.OrderBy(p => p.EventDate ?? DateTime.MinValue).ToList();

            if (id <= 0 || id > sortedHistory.Count)
            {
                var errorEmbed = _companyHistoryEventEmbedGenerator.BuildError();
                await ctx.RespondAsync(string.Empty, false, errorEmbed);
            }
            else
            {
                var embed = _companyHistoryEventEmbedGenerator.Build(sortedHistory[id - 1]);
                await ctx.RespondAsync(string.Empty, false, embed);
            }
        }
    }
}
