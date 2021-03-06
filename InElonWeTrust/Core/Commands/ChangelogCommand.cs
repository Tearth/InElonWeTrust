﻿using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using InElonWeTrust.Core.Commands.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Services.Cache;
using InElonWeTrust.Core.Services.Changelog;

namespace InElonWeTrust.Core.Commands
{
    [CommandsGroup(GroupType.Miscellaneous)]
    public class ChangelogCommand : BaseCommandModule
    {
        private readonly CacheService _cacheService;
        private readonly ChangelogEmbedGenerator _changelogEmbedGenerator;

        public ChangelogCommand(ChangelogService changelogService, CacheService cacheService, ChangelogEmbedGenerator changelogEmbedGenerator)
        {
            _cacheService = cacheService;
            _changelogEmbedGenerator = changelogEmbedGenerator;

            _cacheService.RegisterDataProvider(CacheContentType.Changelog, async p => await changelogService.GetChangelogAsync());
        }

        [Command("Changelog")]
        [Description("Get the bot changelog.")]
        public async Task ChangelogAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var changelog = await _cacheService.GetAsync<string>(CacheContentType.Changelog);
            var embed = _changelogEmbedGenerator.Build(changelog);

            await ctx.RespondAsync(string.Empty, false, embed);
        }
    }
}
