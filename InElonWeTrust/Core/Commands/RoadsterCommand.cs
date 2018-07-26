using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Services.Cache;
using Oddity;
using Oddity.API.Models.Roadster;

namespace InElonWeTrust.Core.Commands
{
    [Commands(GroupType.Miscellaneous)]
    public class RoadsterCommand
    {
        private readonly OddityCore _oddity;
        private readonly CacheService _cacheService;
        private readonly RoadsterEmbedBuilder _roadsterEmbedBuilder;

        public RoadsterCommand(OddityCore oddity, CacheService cacheService, RoadsterEmbedBuilder roadsterEmbedBuilder)
        {
            _oddity = oddity;
            _cacheService = cacheService;
            _roadsterEmbedBuilder = roadsterEmbedBuilder;

            _cacheService.RegisterDataProvider(CacheContentType.Roadster, async p => await _oddity.Roadster.Get().ExecuteAsync());
        }

        [Command("Roadster")]
        [Aliases("TeslaRoadster")]
        [Description("Get information Tesla Roadster sent on Falcon Heavy (`e!getlaunch 55`).")]
        public async Task CompanyInfo(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var roadsterInfo = await _cacheService.Get<RoadsterInfo>(CacheContentType.Roadster);
            var embed = _roadsterEmbedBuilder.Build(roadsterInfo);

            await ctx.RespondAsync("", false, embed);
        }
    }
}
