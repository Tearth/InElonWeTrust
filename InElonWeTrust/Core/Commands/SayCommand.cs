using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using InElonWeTrust.Core.Commands.Attributes;
using InElonWeTrust.Core.Commands.Definitions;

namespace InElonWeTrust.Core.Commands
{
    [CommandsGroup(GroupType.Miscellaneous)]
    public class SayCommand : BaseCommandModule
    {
        [RequireOwner]
        [Hidden, Command("Say")]
        [Description("Say something as Elon")]
        public async Task Say(CommandContext ctx, string content)
        {
            try
            {
                await ctx.Message.DeleteAsync();
            }
            catch
            {
                // Do nothing, message needs to be deleted manually by the owner
            }

            await ctx.RespondAsync(content);
        }
    }
}
