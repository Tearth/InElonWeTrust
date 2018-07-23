using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.UsefulLinks;

namespace InElonWeTrust.Core.Commands
{
    [Commands(GroupType.Miscellaneous)]
    public class UsefulLinksCommand
    {
        private readonly UsefulLinksService _userfulLunksService;
        private readonly UsefulLinksEmbedGenerator _usefulLinksEmbedGenerator;

        public UsefulLinksCommand(UsefulLinksService userfulLunksService, UsefulLinksEmbedGenerator usefulLinksEmbedGenerator)
        {
            _userfulLunksService = userfulLunksService;
            _usefulLinksEmbedGenerator = usefulLinksEmbedGenerator;
        }

        [Command("Links")]
        [Description("Get list of useful links.")]
        public async Task Uptime(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var links = _userfulLunksService.GetUsefulLinks();
            var embed = _usefulLinksEmbedGenerator.Build(links);

            await ctx.RespondAsync(embed: embed);
        }
    }
}
