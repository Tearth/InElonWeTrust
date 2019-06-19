using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Services.Cache;
using InElonWeTrust.Core.Services.Changelog;
using InElonWeTrust.Core.Services.Twitter;

namespace InElonWeTrust.Core.Commands
{
    [Commands(GroupType.Miscellaneous)]
    public class AvatarCommand : BaseCommandModule
    {
        private readonly TwitterService _twitterService;
        private readonly AvatarEmbedGenerator _avatarEmbedGenerator;

        public AvatarCommand(TwitterService twitterService, AvatarEmbedGenerator avatarEmbedGenerator)
        {
            _twitterService = twitterService;
            _avatarEmbedGenerator = avatarEmbedGenerator;
        }

        [Command("Avatar")]
        [Description("Get Elon's Twitter avatar.")]
        public async Task Avatar(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var avatar = _twitterService.GetAvatar(TwitterUserType.ElonMusk);
            var embed = _avatarEmbedGenerator.Build("Elon Musk's", avatar);

            await ctx.RespondAsync("", false, embed);
        }
    }
}
