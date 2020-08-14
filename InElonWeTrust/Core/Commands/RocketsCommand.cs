using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using InElonWeTrust.Core.Commands.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Services.Cache;
using Oddity;
using Oddity.Models.Rockets;

namespace InElonWeTrust.Core.Commands
{
    [CommandsGroup(GroupType.Miscellaneous)]
    public class RocketsCommand : BaseCommandModule
    {
        private readonly CacheService _cacheService;
        private readonly RocketsEmbedGenerator _rocketsEmbedGenerator;

        public RocketsCommand(OddityCore oddity, CacheService cacheService, RocketsEmbedGenerator rocketsEmbedGenerator)
        {
            _cacheService = cacheService;
            _rocketsEmbedGenerator = rocketsEmbedGenerator;

            _cacheService.RegisterDataProvider(CacheContentType.Rockets, async p => await oddity.RocketsEndpoint.GetAll().ExecuteAsync());
        }

        [Command("Rockets"), Aliases("GetRockets")]
        [Description("Get a list of all SpaceX rockets.")]
        public async Task RocketsAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var rockets = await _cacheService.GetAsync<List<RocketInfo>>(CacheContentType.Rockets);
            var embed = _rocketsEmbedGenerator.Build(rockets);

            await ctx.RespondAsync(embed: embed);
        }
    }
}
