using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Cache;
using InElonWeTrust.Core.Services.Pagination;
using Oddity;
using Oddity.API.Models.Company;

namespace InElonWeTrust.Core.Commands
{
    [Commands(":question:", "Misc", "Other strange commands")]
    public class CompanyInfoCommand
    {
        private OddityCore _oddity;
        private PaginationService _paginationService;
        private CacheService _cacheService;

        private const int _idLength = 4;
        private const int _dateLength = 23;
        private const int _titleLength = 45;
        private int _totalLength => _idLength + _dateLength + _titleLength;

        public CompanyInfoCommand(OddityCore oddity, PaginationService paginationService, CacheService cacheService)
        {
            _oddity = oddity;
            _paginationService = paginationService;
            _cacheService = cacheService;

            Bot.Client.MessageReactionAdded += ClientOnMessageReactionAdded;

            _cacheService.RegisterDataProvider(CacheContentType.CompanyInfoHistory, async (p) => await _oddity.Company.GetHistory().ExecuteAsync());
        }

        [Command("CompanyInfo")]
        [Aliases("Company", "ci")]
        [Description("Get information about SpaceX.")]
        public async Task CompanyInfo(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var companyInfo = await _oddity.Company.GetInfo().ExecuteAsync();
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor),
                ThumbnailUrl = Constants.SpaceXLogoImage
            };

            embed.AddField(companyInfo.Name, companyInfo.Summary + " [Read more on Wikipedia](https://en.wikipedia.org/wiki/SpaceX).");
            embed.AddField("CEO", $"[{companyInfo.Ceo}](https://en.wikipedia.org/wiki/Elon_Musk)", true);
            embed.AddField("COO", $"[{companyInfo.Coo}](https://en.wikipedia.org/wiki/Gwynne_Shotwell)", true);
            embed.AddField("CTO", $"[{companyInfo.Cto}](https://en.wikipedia.org/wiki/Elon_Musk)", true);
            embed.AddField("CTO Propolusion", $"[{companyInfo.CtoPropulsion}](https://en.wikipedia.org/wiki/Tom_Mueller)", true);

            embed.AddField("Founded year", companyInfo.FoundedYear.Value.ToString(), true);
            embed.AddField("Employees", companyInfo.Employees.ToString(), true);
            embed.AddField("Launch sites", companyInfo.LaunchSites.ToString(), true);
            embed.AddField("Vehicles", companyInfo.Vehicles.ToString(), true);

            embed.AddField("Headquarters",
                $"[{companyInfo.Headquarters.City}, {companyInfo.Headquarters.State}, {companyInfo.Headquarters.Address}]" +
                $"(https://www.google.com/maps/place/Rocket+Rd,+Hawthorne,+CA+90250,+Stany+Zjednoczone/@33.9213093,-118.3301254,17z/data=!3m1!4b1!4m5!3m4!1s0x80c2b5ded9a490b5:0x3095ae5795c500b3!8m2!3d33.9213093!4d-118.3279367)");

            await ctx.RespondAsync("", false, embed);
        }

        [Command("CompanyHistory")]
        [Aliases("History", "ch")]
        [Description("Get information about SpaceX.")]
        public async Task CompanyHistory(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var companyInfo = await _oddity.Company.GetHistory().ExecuteAsync();
            var launchesList = GetHistoryTable(companyInfo, 1);

            var message = await ctx.RespondAsync(launchesList);
            await _paginationService.InitPagination(message, CacheContentType.CompanyInfoHistory, "");
        }
        
        [Command("GetEvent")]
        [Aliases("Event", "e")]
        [Description("Get information about event with specified id (e!CompanyHistory).")]
        public async Task GetEvent(CommandContext ctx, int id)
        {
            await ctx.TriggerTypingAsync();

            var history = (await _oddity.Company.GetHistory().ExecuteAsync()).OrderBy(p => p.EventDate.Value).ToList();
            if (id <= 0 || id > history.Count)
            {
                var errorEmbedBuilder = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor(Constants.EmbedErrorColor)
                };

                errorEmbedBuilder.AddField("Error", "History event with the specified id doesn't exists, type `e!CompanyHistory` to list them.");

                await ctx.RespondAsync("", false, errorEmbedBuilder);
                return;
            }

            var eventEmbedBuilder = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor)
            };

            var historyEvent = history[id - 1];

            eventEmbedBuilder.AddField(historyEvent.Title, historyEvent.Details.ShortenString(1021));
            eventEmbedBuilder.AddField("Date", historyEvent.EventDate.Value.ToString("F"), true);
            eventEmbedBuilder.AddField("Links", GetLinksData(historyEvent), true);

            await ctx.RespondAsync("", false, eventEmbedBuilder);
        }

        private string GetHistoryTable(List<HistoryEvent> history, int currentPage)
        {
            var historyBuilder = new StringBuilder();
            historyBuilder.Append("```");

            historyBuilder.Append("No. ".PadRight(_idLength));
            historyBuilder.Append("Date".PadRight(_dateLength));
            historyBuilder.Append("Title".PadRight(_titleLength));
            historyBuilder.Append("\r\n");
            historyBuilder.Append(new string('-', _totalLength));
            historyBuilder.Append("\r\n");

            var itemsToDisplay = _paginationService.GetItemsToDisplay(history, currentPage);
            itemsToDisplay = itemsToDisplay.OrderBy(p => p.EventDate.Value).ToList();

            var i = (currentPage - 1) * PaginationService.ItemsPerPage + 1;

            foreach (var historyEvent in itemsToDisplay)
            {
                historyBuilder.Append($"{i}.".PadRight(_idLength));
                historyBuilder.Append(historyEvent.EventDate.Value.ToString("G").PadRight(_dateLength));
                historyBuilder.Append(historyEvent.Title.PadRight(_titleLength));
                historyBuilder.Append("\r\n");

                i++;
            }

            historyBuilder.Append("\r\n");
            historyBuilder.Append("Type e!getevent <number> to get more information.");

            var maxPagesCount = _paginationService.GetPagesCount(history.Count);
            var paginationFooter = _paginationService.GetPaginationFooter(currentPage, maxPagesCount);

            historyBuilder.Append("\r\n");
            historyBuilder.Append(paginationFooter);
            historyBuilder.Append("```");
            return historyBuilder.ToString();
        }

        private string GetLinksData(HistoryEvent historyEvent)
        {
            var links = new List<string>();

            if (historyEvent.Links.Wikipedia != null)
            {
                links.Add($"[Wikipedia]({historyEvent.Links.Wikipedia})");
            }

            if (historyEvent.Links.Reddit != null)
            {
                links.Add($"[Reddit]({historyEvent.Links.Reddit})");
            }

            if (historyEvent.Links.Article != null)
            {
                links.Add($"[Article]({historyEvent.Links.Article})");
            }

            return string.Join(", ", links);
        }

        private async Task ClientOnMessageReactionAdded(MessageReactionAddEventArgs e)
        {
            if (!e.User.IsBot && _paginationService.IsPaginationSet(e.Message))
            {
                var paginationData = _paginationService.GetPaginationDataForMessage(e.Message);

                if (paginationData.ContentType == CacheContentType.CompanyInfoHistory)
                {
                    var items = await _cacheService.Get<List<HistoryEvent>>(CacheContentType.CompanyInfoHistory);

                    if (_paginationService.DoAction(e.Message, e.Emoji, items.Count))
                    {
                        var updatedPaginationData = _paginationService.GetPaginationDataForMessage(e.Message);
                        var history = GetHistoryTable(items, updatedPaginationData.CurrentPage);
                        await e.Message.ModifyAsync(history);
                    }

                    await e.Message.DeleteReactionAsync(e.Emoji, e.User);
                }
            }
        }
    }
}

