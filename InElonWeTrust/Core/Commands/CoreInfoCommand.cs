using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Services.Cache;
using Oddity;
using Oddity.API.Models.DetailedCore;

namespace InElonWeTrust.Core.Commands
{
    [Commands(GroupType.Miscellaneous)]
    public class CoreInfoCommand : BaseCommandModule
    {
        private readonly CacheService _cacheService;
        private readonly CoreInfoEmbedGenerator _coreInfoEmbedGenerator;

        public CoreInfoCommand(OddityCore oddity, CacheService cacheService, CoreInfoEmbedGenerator coreInfoEmbedGenerator)
        {
            _cacheService = cacheService;
            _coreInfoEmbedGenerator = coreInfoEmbedGenerator;

            _cacheService.RegisterDataProvider(CacheContentType.CoreInfo, async p => await oddity.DetailedCores.GetAbout(p).ExecuteAsync());
        }

        [Command("CoreInfo"), Aliases("Core", "GetCore")]
        [Description("Get an information about the specified core.")]
        public async Task CoreInfoAsync(CommandContext ctx, [Description("Core serial number (type `e!Cores` to list them all).")] string coreSerial)
        {
            await ctx.TriggerTypingAsync();

            var coreInfo = await _cacheService.GetAsync<DetailedCoreInfo>(CacheContentType.CoreInfo, coreSerial);

            if (coreInfo != null)
            {
                var embed = _coreInfoEmbedGenerator.Build(coreInfo);
                await ctx.RespondAsync(string.Empty, false, embed);
            }
            else
            {
                var errorEmbed = _coreInfoEmbedGenerator.BuildError();
                await ctx.RespondAsync(string.Empty, false, errorEmbed);
            }
        }
    }
}
