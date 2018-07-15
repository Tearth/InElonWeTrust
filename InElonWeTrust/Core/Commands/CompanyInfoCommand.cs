using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Pagination;
using Oddity;

namespace InElonWeTrust.Core.Commands
{
    [Commands(":question:", "Misc", "Other strange commands")]
    public class CompanyInfoCommand
    {
        private OddityCore _oddity;

        public CompanyInfoCommand()
        {
            _oddity = new OddityCore();
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

            embed.AddField(companyInfo.Name, companyInfo.Summary);
            embed.AddField("CEO", companyInfo.Ceo, true);
            embed.AddField("COO", companyInfo.Coo, true);
            embed.AddField("CTO", companyInfo.Cto, true);
            embed.AddField("CTO Propolusion", companyInfo.CtoPropulsion, true);

            embed.AddField("Founded year", companyInfo.FoundedYear.Value.ToString(), true);
            embed.AddField("Employees", companyInfo.Employees.ToString(), true);
            embed.AddField("Launch sites", companyInfo.LaunchSites.ToString(), true);
            embed.AddField("Vehicles", companyInfo.Vehicles.ToString(), true);

            embed.AddField("Headquarters", $"{companyInfo.Headquarters.City}, {companyInfo.Headquarters.State}, {companyInfo.Headquarters.Address}");

            await ctx.RespondAsync("", false, embed);
        }
    }
}
