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
    public class CompanyInfoCommand : BaseCommandModule
    {
        private readonly CacheService _cacheService;
        private readonly CompanyInfoEmbedGenerator _companyInfoEmbedGenerator;

        public CompanyInfoCommand(OddityCore oddity, CacheService cacheService, CompanyInfoEmbedGenerator companyInfoEmbedGenerator)
        {
            _cacheService = cacheService;
            _companyInfoEmbedGenerator = companyInfoEmbedGenerator;

            _cacheService.RegisterDataProvider(CacheContentType.CompanyInfo, async p => await oddity.Company.GetInfo().ExecuteAsync());
        }

        [Command("CompanyInfo")]
        [Aliases("Company", "ci", "info")]
        [Description("Get the most important information about SpaceX.")]
        public async Task CompanyInfoAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var companyInfo = await _cacheService.GetAsync<CompanyInfo>(CacheContentType.CompanyInfo);
            var embed = _companyInfoEmbedGenerator.Build(companyInfo);

            await ctx.RespondAsync(string.Empty, false, embed);
        }
    }
}

