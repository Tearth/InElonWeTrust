﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Helpers;
using Oddity.API.Models.Roadster;
using Oddity.API.Models.Rocket;

namespace InElonWeTrust.Core.EmbedGenerators
{
    public class RoadsterEmbedBuilder
    {
        private const string StarmanImageUrl = "https://i.imgur.com/wQg7DBS.jpg";
        private readonly DateTime LaunchDate = new DateTime(2018, 2, 6);

        public DiscordEmbedBuilder Build(RoadsterInfo roadster)
        {
            var daysFromLaunch = (int)(DateTime.Now - LaunchDate).TotalDays;

            var embedBuilder = new DiscordEmbedBuilder
            {
                Title = roadster.Name,
                Url = "http://www.whereisroadster.com/",
                Color = new DiscordColor(Constants.EmbedColor),
                ImageUrl = StarmanImageUrl,
                Footer = new DiscordEmbedBuilder.EmbedFooter {Text = $"Roadster has been launched {daysFromLaunch} days ago."}
            };

            embedBuilder.AddField("Speed", $"{roadster.SpeedKph:# ### ###}  kph", true);
            embedBuilder.AddField("Period", $"{(int)roadster.PeriodDays} days", true);

            embedBuilder.AddField("Distance to Earth", $"{roadster.EarthDistanceKm:# ### ### ###} km", true);
            embedBuilder.AddField("Distance to Mars", $"{roadster.MarsDistanceKm:# ### ### ###} km", true);

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
