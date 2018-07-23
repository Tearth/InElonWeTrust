using System.Collections.Generic;
using System.Globalization;
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
using Oddity.API.Models.Launchpad;

namespace InElonWeTrust.Core.Commands
{
    [Commands(GroupType.Miscellaneous)]
    public class LaunchpadsCommand
    {
        private readonly OddityCore _oddity;
        private readonly CacheService _cacheService;
        private readonly LaunchpadsEmbedGenerator _launchpadsEmbedGenerator;

        public LaunchpadsCommand(OddityCore oddity, CacheService cacheService, LaunchpadsEmbedGenerator launchpadsEmbedGenerator)
        {
            _oddity = oddity;
            _cacheService = cacheService;
            _launchpadsEmbedGenerator = launchpadsEmbedGenerator;

            _cacheService.RegisterDataProvider(CacheContentType.Launchpads, async p => await _oddity.Launchpads.GetAll().ExecuteAsync());
        }

        [Command("Launchpads")]
        [Description("Get list of all SpaceX launchpads.")]
        public async Task Launchpads(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var launchpads = await _cacheService.Get<List<LaunchpadInfo>>(CacheContentType.Launchpads);
            var embed = _launchpadsEmbedGenerator.Build(launchpads);

            await ctx.RespondAsync("", false, embed);
        }
    }
}
