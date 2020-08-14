using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using InElonWeTrust.Core.Commands.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Services.Cache;
using Oddity;
using Oddity.Models.Cores;

namespace InElonWeTrust.Core.Commands
{
    [CommandsGroup(GroupType.Miscellaneous)]
    public class CoreInfoCommand : BaseCommandModule
    {
        private readonly CacheService _cacheService;
        private readonly CoreInfoEmbedGenerator _coreInfoEmbedGenerator;

        public CoreInfoCommand(OddityCore oddity, CacheService cacheService, CoreInfoEmbedGenerator coreInfoEmbedGenerator)
        {
            _cacheService = cacheService;
            _coreInfoEmbedGenerator = coreInfoEmbedGenerator;

            _cacheService.RegisterDataProvider(CacheContentType.CoreInfo, async p =>
            {
                var result = await oddity.CoresEndpoint.Query()
                    .WithFieldEqual(q => q.Serial, p)
                    .ExecuteAsync();
                return result.Data.FirstOrDefault();
            });
        }

        [Command("GetCore"), Aliases("Core", "CoreInfo")]
        [Description("Get an information about the specified core.")]
        public async Task GetCoreAsync(CommandContext ctx, [Description("Core serial number (type `e!Cores` to list them all).")] string coreSerial)
        {
            await ctx.TriggerTypingAsync();

            var coreInfo = await _cacheService.GetAsync<CoreInfo>(CacheContentType.CoreInfo, coreSerial);

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
