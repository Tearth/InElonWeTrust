using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Database;

namespace InElonWeTrust.Core.Commands
{
    [Commands(":question:", "Misc", "Other strange commands")]
    public class RandomVideoCommand
    {
        [Command("RandomVideo")]
        [Aliases("Video", "rv")]
        [Description("Get random video related with SpaceX.")]
        public async Task RandomVideo(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            using (var databaseContext = new DatabaseContext())
            {
                var video = databaseContext.VideoLinks.OrderBy(p => Guid.NewGuid()).First();
                await ctx.RespondAsync(video.Link);
            }
        }
    }
}
