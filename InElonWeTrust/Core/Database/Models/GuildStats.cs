using System;
using System.Collections.Generic;
using System.Text;

namespace InElonWeTrust.Core.Database.Models
{
    public class GuildStats
    {
        public int Id { get; set; }
        public string GuildId { get; set; }
        public int CommandExecutionsCount { get; set; }

        public GuildStats()
        {

        }

        public GuildStats(string guildId, int commandExecutionsCount)
        {
            GuildId = guildId;
            CommandExecutionsCount = commandExecutionsCount;
        }
    }
}
