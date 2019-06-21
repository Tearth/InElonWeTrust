using System.Collections.Generic;
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
using Oddity.API.Models.DetailedCore;

namespace InElonWeTrust.Core.Commands
{
    [Commands(GroupType.Miscellaneous)]
    public class CoresCommand : BaseCommandModule
    {
        private readonly PaginationService _paginationService;
        private readonly CacheService _cacheService;
        private readonly CoresListTableGenerator _coresListTableGenerator;

        private readonly List<CacheContentType> _allowedPaginationTypes;

        public CoresCommand(OddityCore oddity, PaginationService paginationService, CacheService cacheService, CoresListTableGenerator coresListTableGenerator)
        {
            _paginationService = paginationService;
            _cacheService = cacheService;
            _coresListTableGenerator = coresListTableGenerator;

            _allowedPaginationTypes = new List<CacheContentType>
            {
                CacheContentType.Cores
            };
            _cacheService.RegisterDataProvider(CacheContentType.Cores, async p => await oddity.DetailedCores.GetAll().ExecuteAsync());

            Bot.Client.MessageReactionAdded += ClientOnMessageReactionAddedAsync;
        }

        [Command("Cores")]
        [Aliases("c", "CoresList")]
        [Description("Get a list of all SpaceX cores.")]
        public async Task CoresAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var cores = await _cacheService.Get<List<DetailedCoreInfo>>(CacheContentType.Cores);
            var tableWithPagination = BuildTableWithPagination(cores, 1);

            var message = await ctx.RespondAsync(tableWithPagination);
            await _paginationService.InitPagination(message, CacheContentType.Cores, string.Empty);
        }

        private string BuildTableWithPagination(List<DetailedCoreInfo> cores, int currentPage)
        {
            cores.Sort(CoreLaunchDateComparer);

            var itemsToDisplay = _paginationService.GetItemsToDisplay(cores, currentPage);

            var maxPagesCount = _paginationService.GetPagesCount(cores.Count);
            var paginationFooter = _paginationService.GetPaginationFooter(currentPage, maxPagesCount);

            return _coresListTableGenerator.Build(itemsToDisplay, currentPage, paginationFooter);
        }

        private async Task ClientOnMessageReactionAddedAsync(MessageReactionAddEventArgs e)
        {
            if (e.User.IsBot || !await _paginationService.IsPaginationSet(e.Message))
            {
                return;
            }

            var paginationData = await _paginationService.GetPaginationDataForMessage(e.Message);
            if (_allowedPaginationTypes.Contains(paginationData.ContentType))
            {
                var items = await _cacheService.Get<List<DetailedCoreInfo>>(CacheContentType.Cores);
                var editedMessage = e.Message;

                if (await _paginationService.DoAction(editedMessage, e.Emoji, items.Count))
                {
                    var updatedPaginationData = await _paginationService.GetPaginationDataForMessage(editedMessage);
                    var tableWithPagination = BuildTableWithPagination(items, updatedPaginationData.CurrentPage);

                    editedMessage = await editedMessage.ModifyAsync(tableWithPagination);
                }

                await _paginationService.DeleteReaction(editedMessage, e.User, e.Emoji);
            }
        }

        private int CoreLaunchDateComparer(DetailedCoreInfo a, DetailedCoreInfo b)
        {
            if (a.OriginalLaunch.HasValue && !b.OriginalLaunch.HasValue)
            {
                return -1;
            }

            if (b.OriginalLaunch.HasValue && !a.OriginalLaunch.HasValue)
            {
                return 1;
            }

            if (a.OriginalLaunch.HasValue && b.OriginalLaunch.HasValue)
            {
                return a.OriginalLaunch.Value > b.OriginalLaunch.Value ? 1 : -1;
            }

            return 0;
        }
    }
}
