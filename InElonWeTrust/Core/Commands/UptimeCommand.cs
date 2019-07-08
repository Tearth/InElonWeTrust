using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;

namespace InElonWeTrust.Core.Commands
{
    [CommandsGroup(GroupType.Miscellaneous)]
    public class UptimeCommand : BaseCommandModule
    {
        private readonly DateTime _startTime;

        public UptimeCommand()
        {
            _startTime = DateTime.Now;
        }

        [Command("Uptime")]
        [Description("Get the bot uptime")]
        public async Task UptimeAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var uptime = DateTime.Now - _startTime;
            var formattedTime = $"{uptime.Days} days, {uptime.Hours} hours, {uptime.Minutes} minutes, {uptime.Seconds} seconds";

            await ctx.RespondAsync($"Uptime: {formattedTime}");
        }
    }
}
