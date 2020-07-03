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

        [Command("Sn")]
        [Description("Get the latest SN status.")]
        public async Task AvatarAsync(CommandContext ctx, int? number)
        {
            await ctx.TriggerTypingAsync();
            var text = _snService.GetSnText();
            await ctx.RespondAsync(text);
        }
    }
}