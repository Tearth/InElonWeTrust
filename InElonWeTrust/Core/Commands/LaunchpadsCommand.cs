﻿using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using InElonWeTrust.Core.Commands.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Services.Cache;
using Oddity;
using Oddity.Models.Launchpads;

namespace InElonWeTrust.Core.Commands
{
    [CommandsGroup(GroupType.Miscellaneous)]
    public class LaunchpadsCommand : BaseCommandModule
    {
        private readonly CacheService _cacheService;
        private readonly LaunchpadsEmbedGenerator _launchpadsEmbedGenerator;

        public LaunchpadsCommand(OddityCore oddity, CacheService cacheService, LaunchpadsEmbedGenerator launchpadsEmbedGenerator)
        {
            _cacheService = cacheService;
            _launchpadsEmbedGenerator = launchpadsEmbedGenerator;

            _cacheService.RegisterDataProvider(CacheContentType.Launchpads, async p => await oddity.LaunchpadsEndpoint.GetAll().ExecuteAsync());
        }

        [Command("Launchpads"), Aliases("GetLaunchpads", "LaunchpadList")]
        [Description("Get a list of all SpaceX launchpads.")]
        public async Task LaunchpadsAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var launchpads = await _cacheService.GetAsync<List<LaunchpadInfo>>(CacheContentType.Launchpads);
            var embed = _launchpadsEmbedGenerator.Build(launchpads);

            await ctx.RespondAsync(string.Empty, false, embed);
        }
    }
}
