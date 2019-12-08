using System.Collections.Generic;
using System.Globalization;
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
                Title = historyEvent.Title,
                Description = historyEvent.Details.ShortenString(1024),
                Color = new DiscordColor(Constants.EmbedColor)
            };

            eventEmbedBuilder.AddField("Date", historyEvent.EventDate?.ToString("D", CultureInfo.InvariantCulture), true);
            eventEmbedBuilder.AddField("Links", GetLinksData(historyEvent), true);

            return eventEmbedBuilder;
        }


        public DiscordEmbed BuildError()
        {
            return new DiscordEmbedBuilder
            {
                Title = ":octagonal_sign: Oops",
                Description = "History event with the specified id doesn't exists, type `e!CompanyHistory` to list them.",
                Color = new DiscordColor(Constants.EmbedErrorColor)
            };
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
