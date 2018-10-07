using System;
using System.Collections.Generic;
using System.Text;

namespace InElonWeTrust.Core.Services.Diagnostic
{
    public class BotStats
    {
        public string BotId { get; set; }
        public int GuildsCount { get; set; }
        public int MembersCount { get; set; }
        public int ExecutedCommandsCount { get; set; }
    }
}
