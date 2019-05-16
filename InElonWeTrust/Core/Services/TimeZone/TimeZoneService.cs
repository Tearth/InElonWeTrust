using System;
using InElonWeTrust.Core.Database;
using System.Linq;
using System.Threading.Tasks;
using InElonWeTrust.Core.Database.Models;
using TimeZoneConverter;

namespace InElonWeTrust.Core.Services.TimeZone
{
    public class TimeZoneService
    {
        public bool TimeZoneExists(string name)
        {
            return TZConvert.TryGetTimeZoneInfo(name, out _);
        }

        public string GetTimeZoneForGuild(ulong guildId)
        {
            var fixedGuildId = guildId.ToString();
            using (var databaseContext = new DatabaseContext())
            {
                var existingConfig = databaseContext.TimeZoneSettings.FirstOrDefault(p => p.GuildId == fixedGuildId);
                return existingConfig?.TimeZoneName;
            }
        }

        public async Task SetTimeZoneAsync(ulong guildId, string timeZoneName)
        {
            var fixedGuildId = guildId.ToString();
            using (var databaseContext = new DatabaseContext())
            {
                var existingConfig = databaseContext.TimeZoneSettings.FirstOrDefault(p => p.GuildId == fixedGuildId);
                if (existingConfig == null)
                {
                    databaseContext.TimeZoneSettings.Add(new TimeZoneSettings(fixedGuildId, timeZoneName));
                }
                else
                {
                    existingConfig.TimeZoneName = timeZoneName;
                }

                await databaseContext.SaveChangesAsync();
            }
        }

        public async Task ResetTimeZoneAsync(ulong guildId)
        {
            var fixedGuildId = guildId.ToString();
            using (var databaseContext = new DatabaseContext())
            {
                var existingConfig = databaseContext.TimeZoneSettings.FirstOrDefault(p => p.GuildId == fixedGuildId);
                if (existingConfig != null)
                {
                    databaseContext.TimeZoneSettings.Remove(existingConfig);
                }

                await databaseContext.SaveChangesAsync();
            }
        }

        public DateTime? ConvertUTCToLocalTime(ulong guildId, DateTime utc)
        {
            var fixedGuildId = guildId.ToString();
            using (var databaseContext = new DatabaseContext())
            {
                var config = databaseContext.TimeZoneSettings.FirstOrDefault(p => p.GuildId == fixedGuildId);
                if (config == null)
                {
                    return null;
                }

                var timeZoneInfo = TZConvert.GetTimeZoneInfo(config.TimeZoneName);
                return TimeZoneInfo.ConvertTimeFromUtc(utc, timeZoneInfo);
            }
        }
    }
}
