using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Services.Cache;
using Oddity;
using Oddity.API.Models.Company;
using Oddity.API.Models.DetailedCore;

namespace InElonWeTrust.Core.Commands
{
    [Commands(GroupType.Miscellaneous)]
    public class CoreInfoCommand
    {
        private readonly OddityCore _oddity;
        private readonly CacheService _cacheService;
        private readonly CoreInfoEmbedGenerator _coreInfoEmbedGenerator;

        public CoreInfoCommand(OddityCore oddity, CacheService cacheService, CoreInfoEmbedGenerator coreInfoEmbedGenerator)
        {
            _oddity = oddity;
            _cacheService = cacheService;
            _coreInfoEmbedGenerator = coreInfoEmbedGenerator;

            _cacheService.RegisterDataProvider(CacheContentType.CoreInfo, async p => await _oddity.DetailedCores.GetAbout(p).ExecuteAsync());
        }

        [Command("CoreInfo")]
        [Aliases("Core", "GetCore")]
        [Description("Get information about the specified core.")]
        public async Task CoreInfo(CommandContext ctx, [Description("Core serial number.")] string coreSerial)
        {
            await ctx.TriggerTypingAsync();

            var coreInfo = await _cacheService.Get<DetailedCoreInfo>(CacheContentType.CoreInfo, coreSerial);

            if (coreInfo != null)
            {
                var embed = _coreInfoEmbedGenerator.Build(coreInfo);
                await ctx.RespondAsync("", false, embed);
            }
            else
            {
                var errorEmbed = _coreInfoEmbedGenerator.BuildError();
                await ctx.RespondAsync("", false, errorEmbed);
            }
        }
    }
}
