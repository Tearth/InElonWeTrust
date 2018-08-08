using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.Services.Cache;
using InElonWeTrust.Core.Services.Pagination;
using InElonWeTrust.Core.TableGenerators;
using NLog;
using Oddity;
using Oddity.API.Models.DetailedCore;

namespace InElonWeTrust.Core.Commands
{
    [Commands(GroupType.Miscellaneous)]
    public class CoresCommand
    {
        private readonly OddityCore _oddity;
        private readonly PaginationService _paginationService;
        private readonly CacheService _cacheService;
        private readonly CoresListTableGenerator _coresListTableGenerator;

        private readonly List<CacheContentType> _allowedPaginationTypes;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public CoresCommand(OddityCore oddity, PaginationService paginationService, CacheService cacheService, CoresListTableGenerator coresListTableGenerator)
        {
            _oddity = oddity;
            _paginationService = paginationService;
            _cacheService = cacheService;
            _coresListTableGenerator = coresListTableGenerator;

            _allowedPaginationTypes = new List<CacheContentType>
            {
                CacheContentType.Cores
            };
            _cacheService.RegisterDataProvider(CacheContentType.Cores, async p => await _oddity.DetailedCores.GetAll().ExecuteAsync());

            Bot.Client.MessageReactionAdded += ClientOnMessageReactionAdded;
        }

        [Command("Cores")]
        [Aliases("c")]
        [Description("Get list of all SpaceX cores.")]
        public async Task Cores(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var cores = await _cacheService.Get<List<DetailedCoreInfo>>(CacheContentType.Cores);
            var tableWithPagination = BuildTableWithPagination(cores, 1);

            var message = await ctx.RespondAsync(tableWithPagination);
            await _paginationService.InitPagination(message, CacheContentType.Cores, "");
        }

        private string BuildTableWithPagination(List<DetailedCoreInfo> cores, int currentPage)
        {
            var itemsWithoutOriginalLaunch = cores.Where(p => !p.OriginalLaunch.HasValue).ToList();
            foreach (var itemToRemove in itemsWithoutOriginalLaunch)
            {
                cores.Remove(itemToRemove);
            }
            cores.AddRange(itemsWithoutOriginalLaunch);

            var itemsToDisplay = _paginationService.GetItemsToDisplay(cores, currentPage);

            var maxPagesCount = _paginationService.GetPagesCount(cores.Count);
            var paginationFooter = _paginationService.GetPaginationFooter(currentPage, maxPagesCount);

            return _coresListTableGenerator.Build(itemsToDisplay, currentPage, paginationFooter);
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
                var items = await _cacheService.Get<List<DetailedCoreInfo>>(CacheContentType.Cores);
                var editedMessage = e.Message;

                if (_paginationService.DoAction(e.Message, e.Emoji, items.Count))
                {
                    var updatedPaginationData = _paginationService.GetPaginationDataForMessage(e.Message);
                    var tableWithPagination = BuildTableWithPagination(items, updatedPaginationData.CurrentPage);

                    editedMessage = await e.Message.ModifyAsync(tableWithPagination);
                }

                try
                {
                    await e.Message.DeleteReactionAsync(e.Emoji, e.User);
                }
                catch (UnauthorizedException)
                {
                    var oldMessageContent = editedMessage.Content ?? (await e.Channel.GetMessageAsync(e.Message.Id)).Content;

                    if (oldMessageContent.EndsWith("```", StringComparison.InvariantCultureIgnoreCase))
                    {
                        oldMessageContent += "\r\n";
                        oldMessageContent += "*It seems that I have no enough permissions to do pagination properly. Please check " +
                                             "bot/channel permissions and be sure that I have ability to manage messages.*";
                    }
                    _logger.Warn("Can't do pagination due to permissions.");
                    await editedMessage.ModifyAsync(oldMessageContent);
                }
            }
        }
    }
}
