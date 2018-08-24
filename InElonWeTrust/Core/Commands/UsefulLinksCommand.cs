using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Services.UsefulLinks;

namespace InElonWeTrust.Core.Commands
{
    [Commands(GroupType.Miscellaneous)]
    public class UsefulLinksCommand : BaseCommandModule
    {
        private readonly UsefulLinksService _usefulLinksService;
        private readonly UsefulLinksEmbedGenerator _usefulLinksEmbedGenerator;

        public UsefulLinksCommand(UsefulLinksService usefulLinksService, UsefulLinksEmbedGenerator usefulLinksEmbedGenerator)
        {
            _usefulLinksService = usefulLinksService;
            _usefulLinksEmbedGenerator = usefulLinksEmbedGenerator;
        }

        [Command("Links")]
        [Description("Get list of useful links.")]
        public async Task Uptime(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var links = _usefulLinksService.GetUsefulLinks();
            var embed = _usefulLinksEmbedGenerator.Build(links);

            await ctx.RespondAsync(embed: embed);
        }
    }
}
