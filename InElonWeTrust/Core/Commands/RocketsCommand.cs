using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Oddity.API.Models.Rocket;

namespace InElonWeTrust.Core.Commands
{
    [Commands(GroupType.Miscellaneous)]
    public class RocketsCommand
    {
        private readonly OddityCore _oddity;
        private readonly CacheService _cacheService;
        private readonly RocketsEmbedGenerator _rocketsEmbedGenerator;

        public RocketsCommand(OddityCore oddity, CacheService cacheService, RocketsEmbedGenerator rocketsEmbedGenerator)
        {
            _oddity = oddity;
            _cacheService = cacheService;
            _rocketsEmbedGenerator = rocketsEmbedGenerator;

            _cacheService.RegisterDataProvider(CacheContentType.Rockets, async p => await _oddity.Rockets.GetAll().ExecuteAsync());
        }

        [Command("Rockets")]
        [Description("Get list of all SpaceX rockets.")]
        public async Task Rockets(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var rockets = await _cacheService.Get<List<RocketInfo>>(CacheContentType.Rockets);
            var embed = _rocketsEmbedGenerator.Build(rockets);

            await ctx.RespondAsync(embed: embed);
        }
    }
}
