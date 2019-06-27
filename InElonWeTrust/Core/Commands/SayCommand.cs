using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.Settings;

namespace InElonWeTrust.Core.Commands
{
    [Commands(GroupType.Miscellaneous)]
    public class SayCommand : BaseCommandModule
    {
        [Command("Say")]
        [Description("Say something as Elon")]
        [HiddenCommand]
        public async Task Say(CommandContext ctx, string content)
        {
            if (ctx.User.Id != SettingsLoader.Data.OwnerId)
            {
                return;
            }

            await ctx.Message.DeleteAsync();
            await ctx.RespondAsync(content);
        }
    }
}
