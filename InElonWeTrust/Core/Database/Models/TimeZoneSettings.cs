namespace InElonWeTrust.Core.Database.Models
{
    public class TimeZoneSettings
    {
        public int Id { get; set; }
        public string GuildId { get; set; }
        public string TimeZoneName { get; set; }

        public TimeZoneSettings(string guildId, string timeZoneName)
        {
            GuildId = guildId;
            TimeZoneName = timeZoneName;
        }
    }
}
