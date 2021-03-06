﻿using System;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Helpers.Extensions;
using InElonWeTrust.Core.Settings;
using NLog;
using NLog.Targets;

namespace InElonWeTrust.Core.Helpers.Logger
{
    public sealed class DiscordLogTarget : TargetWithLayout
    {
        protected override async void Write(LogEventInfo logEvent)
        {
            try
            {
                var logMessage = Layout.Render(logEvent);

                if (Bot.Client != null)
                {
                    var embed = new DiscordEmbedBuilder { Color = new DiscordColor(Constants.EmbedErrorColor) };
                    embed.AddField("Error", logMessage.ShortenString(1024));

                    var supportGuild = await Bot.Client.GetGuildAsync(SettingsLoader.Data.SupportServerId);
                    var ownerMember = await supportGuild.GetMemberAsync(SettingsLoader.Data.OwnerId);

                    await ownerMember.SendMessageAsync(string.Empty, false, embed);
                }
            }
            catch (Exception e)
            {
                // Well, there is nothing more we can do here.
                Console.WriteLine(e);
            }
        }
    }
}
