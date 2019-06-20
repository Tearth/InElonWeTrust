using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Cache;
using InElonWeTrust.Core.Services.LaunchNotifications;
using Oddity;
using Oddity.API.Models.Launch;

namespace InElonWeTrust.Core.Commands
{
    [Commands(GroupType.Launches)]
    public class SingleLaunchCommands : BaseCommandModule
    {
        private readonly OddityCore _oddity;
        private readonly CacheService _cacheService;
        private readonly LaunchNotificationsService _launchNotificationsService;
        private readonly LaunchInfoEmbedGenerator _launchInfoEmbedGenerator;

        public SingleLaunchCommands(OddityCore oddity, CacheService cacheService, LaunchNotificationsService launchNotificationsService, LaunchInfoEmbedGenerator launchInfoEmbedGenerator)
        {
            _oddity = oddity;
            _cacheService = cacheService;
            _launchNotificationsService = launchNotificationsService;
            _launchInfoEmbedGenerator = launchInfoEmbedGenerator;

            _cacheService.RegisterDataProvider(CacheContentType.AllLaunches, async p => await _oddity.Launches.GetAll().ExecuteAsync());
            _cacheService.RegisterDataProvider(CacheContentType.NextLaunch, async p => await _oddity.Launches.GetNext().ExecuteAsync());
            _cacheService.RegisterDataProvider(CacheContentType.LatestLaunch, async p => await _oddity.Launches.GetLatest().ExecuteAsync());
        }

        [Command("NextLaunch")]
        [Aliases("Next", "nl")]
        [Description("Get information about the next launch.")]
        public async Task NextLaunch(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var launchData = await _cacheService.Get<LaunchInfo>(CacheContentType.NextLaunch);
            var embed = _launchInfoEmbedGenerator.Build(launchData, ctx.Guild.Id, true);

            var sentMessage = await ctx.RespondAsync(string.Empty, false, embed);

            await sentMessage.CreateReactionAsync(DiscordEmoji.FromName(Bot.Client, ":regional_indicator_s:"));
            _launchNotificationsService.AddMessageToSubscribe(ctx.Channel, sentMessage);
        }

        [Command("LatestLaunch")]
        [Aliases("Latest", "ll")]
        [Description("Get information about the latest launch.")]
        public async Task LatestLaunch(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var launchData = await _cacheService.Get<LaunchInfo>(CacheContentType.LatestLaunch);

            var embed = _launchInfoEmbedGenerator.Build(launchData, ctx.Guild.Id, false);
            await ctx.RespondAsync(string.Empty, false, embed);
        }

        [Command("RandomLaunch")]
        [Aliases("Random", "rl")]
        [Description("Get information about random launch.")]
        public async Task RandomLaunch(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var launchData = await _cacheService.Get<List<LaunchInfo>>(CacheContentType.AllLaunches);
            var randomLaunch = launchData.OrderBy(p => Guid.NewGuid()).First();

            var embed = _launchInfoEmbedGenerator.Build(randomLaunch, ctx.Guild.Id, false);
            await ctx.RespondAsync(string.Empty, false, embed);
        }

        [Command("GetLaunch")]
        [Aliases("Launch", "gl")]
        [Description("Get information about launch with the specified flight number (which can be obtained by `e!AllLaunches` command).")]
        public async Task GetLaunch(CommandContext ctx, [Description("Launch number (type `e!AllLaunches` to catch them all)")] int id)
        {
            await ctx.TriggerTypingAsync();

            var launchData = await _oddity.Launches.GetAll().WithFlightNumber(id).ExecuteAsync();
            if (!launchData.Any())
            {
                var errorEmbedBuilder = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor(Constants.EmbedErrorColor)
                };

                errorEmbedBuilder.AddField(":octagonal_sign: Error", "Flight with the specified launch number doesn't exist, type `e!alllaunches` to list them.");
                await ctx.RespondAsync(string.Empty, false, errorEmbedBuilder);
            }
            else
            {
                var embed = _launchInfoEmbedGenerator.Build(launchData.First(), ctx.Guild.Id, false);
                await ctx.RespondAsync(string.Empty, false, embed);
            }
        }
    }
}
