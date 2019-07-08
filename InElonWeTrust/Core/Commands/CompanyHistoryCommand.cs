using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.EventArgs;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.Services.Cache;
using InElonWeTrust.Core.Services.Pagination;
using InElonWeTrust.Core.TableGenerators;
using Oddity;
using Oddity.API.Models.Company;

namespace InElonWeTrust.Core.Commands
{
    [CommandsGroup(GroupType.Miscellaneous)]
    public class CompanyHistoryCommand : BaseCommandModule
    {
        private readonly PaginationService _paginationService;
        private readonly CacheService _cacheService;
        private readonly CompanyHistoryTableGenerator _companyHistoryTableGenerator;

        private readonly List<CacheContentType> _allowedPaginationTypes;

        public CompanyHistoryCommand(OddityCore oddity, PaginationService paginationService, CacheService cacheService, CompanyHistoryTableGenerator companyHistoryTableGenerator)
        {
            _paginationService = paginationService;
            _cacheService = cacheService;
            _companyHistoryTableGenerator = companyHistoryTableGenerator;

            _allowedPaginationTypes = new List<CacheContentType>
            {
                CacheContentType.CompanyHistory
            };
            _cacheService.RegisterDataProvider(CacheContentType.CompanyHistory, async p => await oddity.Company.GetHistory().ExecuteAsync());

            Bot.Client.MessageReactionAdded += ClientOnMessageReactionAddedAsync;
        }

        [Command("CompanyHistory"), Aliases("History")]
        [Description("Get a list of the most important events related to SpaceX.")]
        public async Task CompanyHistoryAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var companyHistory = await _cacheService.GetAsync<List<HistoryEvent>>(CacheContentType.CompanyHistory);
            var tableWithPagination = BuildTableWithPagination(companyHistory, 1);

            var message = await ctx.RespondAsync(tableWithPagination);
            await _paginationService.InitPaginationAsync(message, CacheContentType.CompanyHistory, string.Empty);
        }

        private string BuildTableWithPagination(List<HistoryEvent> history, int currentPage)
        {
            var itemsToDisplay = _paginationService.GetItemsToDisplay(history, currentPage);
            itemsToDisplay = itemsToDisplay.OrderBy(p => p.EventDate ?? DateTime.MinValue).ToList();

            var maxPagesCount = _paginationService.GetPagesCount(history.Count);
            var paginationFooter = _paginationService.GetPaginationFooter(currentPage, maxPagesCount);

            return _companyHistoryTableGenerator.Build(itemsToDisplay, currentPage, paginationFooter);
        }

        private async Task ClientOnMessageReactionAddedAsync(MessageReactionAddEventArgs e)
        {
            if (e.User.IsBot || !await _paginationService.IsPaginationSetAsync(e.Message))
            {
                return;
            }

            var paginationData = await _paginationService.GetPaginationDataForMessageAsync(e.Message);
            if (_allowedPaginationTypes.Contains(paginationData.ContentType))
            {
                var items = await _cacheService.GetAsync<List<HistoryEvent>>(CacheContentType.CompanyHistory);
                var editedMessage = e.Message;

                if (await _paginationService.DoActionAsync(editedMessage, e.Emoji, items.Count))
                {
                    var updatedPaginationData = await _paginationService.GetPaginationDataForMessageAsync(editedMessage);
                    var tableWithPagination = BuildTableWithPagination(items, updatedPaginationData.CurrentPage);

                    editedMessage = await editedMessage.ModifyAsync(tableWithPagination);
                }

                await _paginationService.DeleteReactionAsync(editedMessage, e.User, e.Emoji);
            }
        }
    }
}
