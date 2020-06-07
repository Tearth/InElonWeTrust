using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using InElonWeTrust.Core.Commands.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Services.Sn;
using InElonWeTrust.Core.Services.Twitter;

namespace InElonWeTrust.Core.Commands
{
    [CommandsGroup(GroupType.Miscellaneous)]
    public class SnCommand : BaseCommandModule
    {
        private readonly SnService _snService;

        public SnCommand(SnService snService)
        {
            _snService = snService;
        }

        [Hidden]
        [Command("Sn")]
        [Description("Get the latest SN status.")]
        public async Task SnAsync(CommandContext ctx, int? number = null)
        {
            await ctx.TriggerTypingAsync();
            var text = _snService.GetSnText(number);
            await ctx.RespondAsync(text);
        }
    }
}