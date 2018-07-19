using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Database.Models;
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Cache;
using InElonWeTrust.Core.Services.Pagination;
using Oddity;
using Oddity.API.Models.Launch;
using Oddity.API.Models.Launch.Rocket.FirstStage;
using Oddity.API.Models.Launch.Rocket.SecondStage;

namespace InElonWeTrust.Core.Commands
{
    [Commands(":rocket:", "Launches", "Information about all SpaceX launches")]
    public class SingleLaunchCommands
    {
        private OddityCore _oddity;
        private CacheService _cacheService;
        private LaunchInfoEmbedGenerator _launchInfoEmbedGenerator;

        public SingleLaunchCommands(OddityCore oddity, CacheService cacheService, LaunchInfoEmbedGenerator launchInfoEmbedGenerator)
        {
            _oddity = oddity;
            _cacheService = cacheService;
            _launchInfoEmbedGenerator = launchInfoEmbedGenerator;

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
            var embed = await _launchInfoEmbedGenerator.Build(launchData, true);

            var sentMessage = await ctx.RespondAsync("", false, embed);

            await sentMessage.CreateReactionAsync(DiscordEmoji.FromName(Bot.Client, ":regional_indicator_s:"));
            using (var databaseContext = new DatabaseContext())
            {
                databaseContext.MessagesToSubscribe.Add(new MessageToSubscribe(sentMessage.Id.ToString()));
                databaseContext.SaveChanges();
            }
        }

        [Command("LatestLaunch")]
        [Aliases("Latest", "ll")]
        [Description("Get information about the latest launch.")]
        public async Task LatestLaunch(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var launchData = await _cacheService.Get<LaunchInfo>(CacheContentType.LatestLaunch);
            var embed = await _launchInfoEmbedGenerator.Build(launchData, false);
            await ctx.RespondAsync("", false, embed);
        }

        [Command("RandomLaunch")]
        [Aliases("Random", "rl")]
        [Description("Get information about random launch.")]
        public async Task RandomLaunch(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var launchData = await _oddity.Launches.GetAll().ExecuteAsync();
            var randomLaunch = launchData.OrderBy(p => Guid.NewGuid()).First();

            var embed = await _launchInfoEmbedGenerator.Build(randomLaunch, false);
            await ctx.RespondAsync("", false, embed);
        }

        [Command("GetLaunch")]
        [Aliases("Launch", "gl")]
        [Description("Get information about launch with the specified flight number (which can be obtained by `e!AllLaunches command`).")]
        public async Task GetLaunch(CommandContext ctx, [Description("Launch number (type `e!AllLaunches to catch them all)")] int id)
        {
            await ctx.TriggerTypingAsync();

            var launchData = await _oddity.Launches.GetAll().WithFlightNumber(id).ExecuteAsync();
            if (!launchData.Any())
            {
                var errorEmbedBuilder = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor(Constants.EmbedErrorColor)
                };

                errorEmbedBuilder.AddField(":octagonal_sign: Error", "Flight with the specified launch number doesn't exist");
                await ctx.RespondAsync("", false, errorEmbedBuilder);
            }
            else
            {
                var embed = await _launchInfoEmbedGenerator.Build(launchData.First(), false);
                await ctx.RespondAsync("", false, embed);
            }
        }
    }
}
