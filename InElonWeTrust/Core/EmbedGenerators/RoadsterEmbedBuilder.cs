using System;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Helpers;
using Oddity.API.Models.Roadster;

namespace InElonWeTrust.Core.EmbedGenerators
{
    public class RoadsterEmbedBuilder
    {
        private const string StarmanImageUrl = "https://i.imgur.com/wQg7DBS.jpg";
        private readonly DateTime _launchDate = new DateTime(2018, 2, 6);

        public DiscordEmbedBuilder Build(RoadsterInfo roadster)
        {
            var daysFromLaunch = (int)(DateTime.Now - _launchDate).TotalDays;

            var embedBuilder = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor),
                ImageUrl = StarmanImageUrl,
                Footer = new DiscordEmbedBuilder.EmbedFooter {Text = $"Roadster has been launched {daysFromLaunch} days ago."}
            };

            embedBuilder.AddField(roadster.Name, $"{roadster.Details} [[Wikipedia]]({roadster.Wikipedia})");

            embedBuilder.AddField("Speed", $"{roadster.SpeedKph:# ### ###}  kph", true);
            embedBuilder.AddField("Period", $"{(int)roadster.PeriodDays} days", true);

            embedBuilder.AddField("Distance to Earth", $"{roadster.EarthDistanceKilometers:# ### ### ###} km", true);
            embedBuilder.AddField("Distance to Mars", $"{roadster.MarsDistanceKilometers:# ### ### ###} km", true);

            embedBuilder.AddField("Apoapsis", $"{roadster.ApoapsisAu:0.###} au = {roadster.ApoapsisAu * 149_597_871:# ### ### ###} km", true);
            embedBuilder.AddField("Periapsis", $"{roadster.PeriapsisAu:0.###} au = {roadster.PeriapsisAu * 149_597_871:# ### ### ###} km", true);

            embedBuilder.AddField("Inclination", $"{roadster.Inclination:0.###}", true);
            embedBuilder.AddField("Eccentricity", $"{roadster.Eccentricity:0.###}", true);

            embedBuilder.AddField("Longitude", $"{roadster.Longitude:0.###}", true);
            embedBuilder.AddField("Semi-major axis", $"{roadster.SemiMajorAxisAu:0.###}", true);

            return embedBuilder;
        }
    }
}
