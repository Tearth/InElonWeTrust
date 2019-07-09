using System.Collections.Generic;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Helpers.Extensions;
using Oddity.API.Models.Company;

namespace InElonWeTrust.Core.EmbedGenerators
{
    public class CompanyHistoryEventEmbedGenerator
    {
        public DiscordEmbed Build(HistoryEvent historyEvent)
        {
            var eventEmbedBuilder = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor)
            };

            eventEmbedBuilder.AddField(historyEvent.Title, historyEvent.Details.ShortenString(1024));
            eventEmbedBuilder.AddField("Date", historyEvent.EventDate?.ToString("D"), true);
            eventEmbedBuilder.AddField("Links", GetLinksData(historyEvent), true);

            return eventEmbedBuilder;
        }


        public DiscordEmbed BuildError()
        {
            var errorEmbedBuilder = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedErrorColor)
            };

            errorEmbedBuilder.AddField(":octagonal_sign: Oops", "History event with the specified id doesn't exists, type `e!CompanyHistory` to list them.");

            return errorEmbedBuilder;
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
    }
}
