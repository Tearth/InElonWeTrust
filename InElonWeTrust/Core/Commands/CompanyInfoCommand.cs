using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Cache;
using Oddity;
using Oddity.API.Models.Company;

namespace InElonWeTrust.Core.Commands
{
    [Commands(GroupType.Miscellaneous)]
    public class CompanyInfoCommand
    {
        private readonly OddityCore _oddity;
        private readonly CacheService _cacheService;
        private readonly CompanyInfoEmbedGenerator _companyInfoEmbedGenerator;

        public CompanyInfoCommand(OddityCore oddity, CacheService cacheService, CompanyInfoEmbedGenerator companyInfoEmbedGenerator)
        {
            _oddity = oddity;
            _cacheService = cacheService;
            _companyInfoEmbedGenerator = companyInfoEmbedGenerator;
        }

        [Command("CompanyInfo")]
        [Aliases("Company", "ci")]
        [Description("Get information about SpaceX.")]
        public async Task CompanyInfo(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var companyInfo = await _oddity.Company.GetInfo().ExecuteAsync();
            var embed = _companyInfoEmbedGenerator.Build(companyInfo);

            await ctx.RespondAsync("", false, embed);
        }
    }
}

