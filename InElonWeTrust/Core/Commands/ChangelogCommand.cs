using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Services.Changelog;

namespace InElonWeTrust.Core.Commands
{
    [Commands(GroupType.Miscellaneous)]
    public class ChangelogCommand
    {
        private readonly ChangelogService _changelogService;
        private readonly ChangelogEmbedGenerator _changelogEmbedGenerator;

        public ChangelogCommand(ChangelogService changelogService, ChangelogEmbedGenerator changelogEmbedGenerator)
        {
            _changelogService = changelogService;
            _changelogEmbedGenerator = changelogEmbedGenerator;
        }

        [Command("Changelog")]
        [Description("Get bot changelog.")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var changelog = await _changelogService.GetChangelog();
            var embed = _changelogEmbedGenerator.Build(changelog);

            await ctx.RespondAsync("", false, embed);
        }
    }
}
