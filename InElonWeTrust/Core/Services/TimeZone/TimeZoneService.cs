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
    }
}
