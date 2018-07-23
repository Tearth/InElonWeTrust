using System.Collections.Generic;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Helpers;
using Oddity.API.Models.Company;

namespace InElonWeTrust.Core.EmbedGenerators
{
    public class CompanyHistoryEventEmbedGenerator
    {
        public DiscordEmbedBuilder Build(HistoryEvent historyEvent)
        {
            var eventEmbedBuilder = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor)
            };

            eventEmbedBuilder.AddField(historyEvent.Title, historyEvent.Details.ShortenString(1021));
            eventEmbedBuilder.AddField("Date", historyEvent.EventDate.Value.ToString("F"), true);
            eventEmbedBuilder.AddField("Links", GetLinksData(historyEvent), true);

            return eventEmbedBuilder;
        }


        public DiscordEmbedBuilder BuildError()
        {
            var errorEmbedBuilder = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedErrorColor)
            };

            errorEmbedBuilder.AddField("Error", "History event with the specified id doesn't exists, type `e!CompanyHistory` to list them.");

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
