using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.EventArgs;
using InElonWeTrust.Core.Commands.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.Services.Cache;
using InElonWeTrust.Core.Services.Pagination;
using InElonWeTrust.Core.TableGenerators;
using Oddity;
using Oddity.Models.Cores;

namespace InElonWeTrust.Core.Commands
{
    [CommandsGroup(GroupType.Miscellaneous)]
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
            _cacheService.RegisterDataProvider(CacheContentType.Cores, async p => await oddity.CoresEndpoint.GetAll().ExecuteAsync());

            Bot.Client.MessageReactionAdded += ClientOnMessageReactionAddedAsync;
        }

        [Command("Cores"), Aliases("CoresList")]
        [Description("Get a list of all SpaceX cores.")]
        public async Task CoresAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var cores = await _cacheService.GetAsync<List<CoreInfo>>(CacheContentType.Cores);
            var tableWithPagination = BuildTableWithPagination(cores, 1);

            var message = await ctx.RespondAsync(tableWithPagination);
            await _paginationService.InitPaginationAsync(message, CacheContentType.Cores, string.Empty);
        }

        private string BuildTableWithPagination(List<CoreInfo> cores, int currentPage)
        {
            cores.Sort(CoreLaunchDateComparer);

            var itemsToDisplay = _paginationService.GetItemsToDisplay(cores, currentPage);

            var maxPagesCount = _paginationService.GetPagesCount(cores.Count);
            var paginationFooter = _paginationService.GetPaginationFooter(currentPage, maxPagesCount);

            return _coresListTableGenerator.Build(itemsToDisplay, currentPage, paginationFooter);
        }

        private async Task ClientOnMessageReactionAddedAsync(DiscordClient client, MessageReactionAddEventArgs e)
        {
            if (e.User.IsBot || !await _paginationService.IsPaginationSetAsync(e.Message))
            {
                return;
            }

            var paginationData = await _paginationService.GetPaginationDataForMessageAsync(e.Message);
            if (_allowedPaginationTypes.Contains(paginationData.ContentType))
            {
                var items = await _cacheService.GetAsync<List<CoreInfo>>(CacheContentType.Cores);
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

        private int CoreLaunchDateComparer(CoreInfo a, CoreInfo b)
        {
            var firstOriginalLaunch = a.Launches.Count > 0 ? a.Launches[0].Value.DateUtc : null;
            var secondOriginalLaunch = b.Launches.Count > 0 ? b.Launches[0].Value.DateUtc : null;

            if (firstOriginalLaunch.HasValue && !secondOriginalLaunch.HasValue)
            {
                return -1;
            }

            if (secondOriginalLaunch.HasValue && !firstOriginalLaunch.HasValue)
            {
                return 1;
            }

            if (firstOriginalLaunch.HasValue && secondOriginalLaunch.HasValue)
            {
                return firstOriginalLaunch.Value > secondOriginalLaunch.Value ? 1 : -1;
            }

            return 0;
        }
    }
}
