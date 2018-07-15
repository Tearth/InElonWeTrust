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
    }
}
