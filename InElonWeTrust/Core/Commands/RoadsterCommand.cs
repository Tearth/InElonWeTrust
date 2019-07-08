using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using InElonWeTrust.Core.Commands.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Services.Cache;
using Oddity;
using Oddity.API.Models.Roadster;

namespace InElonWeTrust.Core.Commands
{
    [CommandsGroup(GroupType.Miscellaneous)]
    public class RoadsterCommand : BaseCommandModule
    {
        private readonly CacheService _cacheService;
        private readonly RoadsterEmbedBuilder _roadsterEmbedBuilder;

        public RoadsterCommand(OddityCore oddity, CacheService cacheService, RoadsterEmbedBuilder roadsterEmbedBuilder)
        {
            _cacheService = cacheService;
            _roadsterEmbedBuilder = roadsterEmbedBuilder;

            _cacheService.RegisterDataProvider(CacheContentType.Roadster, async p => await oddity.Roadster.Get().ExecuteAsync());
        }

        [Command("Roadster"), Aliases("TeslaRoadster", "Tesla", "Starman")]
        [Description("Get an information about Tesla Roadster sent on Falcon Heavy (`e!getlaunch 55`).")]
        public async Task CompanyInfoAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var roadsterInfo = await _cacheService.GetAsync<RoadsterInfo>(CacheContentType.Roadster);
            var embed = _roadsterEmbedBuilder.Build(roadsterInfo);

            await ctx.RespondAsync(string.Empty, false, embed);
        }
    }
}
