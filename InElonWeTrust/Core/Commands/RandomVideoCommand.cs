using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using InElonWeTrust.Core.Commands.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Helpers.Extensions;

namespace InElonWeTrust.Core.Commands
{
    [CommandsGroup(GroupType.Miscellaneous)]
    public class RandomVideoCommand : BaseCommandModule
    {
        [Command("RandomVideo"), Aliases("Video")]
        [Description("Get a random video related to SpaceX.")]
        public async Task RandomVideoAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            using (var databaseContext = new DatabaseContext())
            {
                var video = databaseContext.VideoLinks.RandomRow("VideoLinks").First();
                await ctx.RespondAsync(video.Link);
            }
        }
    }
}
