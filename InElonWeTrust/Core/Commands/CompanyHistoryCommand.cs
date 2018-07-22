﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    [Commands(GroupType.Miscellaneous)]
    public class CompanyHistoryCommand
    {
        private readonly OddityCore _oddity;
        private readonly PaginationService _paginationService;
        private readonly CacheService _cacheService;
        private readonly CompanyHistoryTableGenerator _companyHistoryTableGenerator;

        private readonly List<CacheContentType> _allowedPaginationTypes;

        public CompanyHistoryCommand(OddityCore oddity, PaginationService paginationService, CacheService cacheService, CompanyHistoryTableGenerator companyHistoryTableGenerator)
        {
            _oddity = oddity;
            _paginationService = paginationService;
            _cacheService = cacheService;
            _companyHistoryTableGenerator = companyHistoryTableGenerator;

            _allowedPaginationTypes = new List<CacheContentType>
            {
                CacheContentType.CompanyHistory
            };
            _cacheService.RegisterDataProvider(CacheContentType.CompanyHistory, async p => await _oddity.Company.GetHistory().ExecuteAsync());

            Bot.Client.MessageReactionAdded += ClientOnMessageReactionAdded;
        }

        [Command("CompanyHistory")]
        [Aliases("History", "ch")]
        [Description("Get information about SpaceX.")]
        public async Task CompanyHistory(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var companyHistory = await _cacheService.Get<List<HistoryEvent>>(CacheContentType.CompanyHistory);
            var tableWithPagination = BuildTableWithPagination(companyHistory, 1);

            var message = await ctx.RespondAsync(tableWithPagination);
            await _paginationService.InitPagination(message, CacheContentType.CompanyHistory, "");
        }

        private string BuildTableWithPagination(List<HistoryEvent> history, int currentPage)
        {
            var itemsToDisplay = _paginationService.GetItemsToDisplay(history, currentPage);
            itemsToDisplay = itemsToDisplay.OrderBy(p => p.EventDate.Value).ToList();

            var maxPagesCount = _paginationService.GetPagesCount(history.Count);
            var paginationFooter = _paginationService.GetPaginationFooter(currentPage, maxPagesCount);

            return _companyHistoryTableGenerator.Build(itemsToDisplay, currentPage, paginationFooter);
        }

        private async Task ClientOnMessageReactionAdded(MessageReactionAddEventArgs e)
        {
            if (e.User.IsBot || !_paginationService.IsPaginationSet(e.Message))
            {
                return;
            }

            var paginationData = _paginationService.GetPaginationDataForMessage(e.Message);
            if (_allowedPaginationTypes.Contains(paginationData.ContentType))
            {
                var items = await _cacheService.Get<List<HistoryEvent>>(CacheContentType.CompanyHistory);

                if (_paginationService.DoAction(e.Message, e.Emoji, items.Count))
                {
                    var updatedPaginationData = _paginationService.GetPaginationDataForMessage(e.Message);
                    var tableWithPagination = BuildTableWithPagination(items, updatedPaginationData.CurrentPage);

                    await e.Message.ModifyAsync(tableWithPagination);
                }

                await e.Message.DeleteReactionAsync(e.Emoji, e.User);
            }
        }
    }
}
