namespace InElonWeTrust.Core.Database.Models
{
    public class CommandStats
    {
        public int Id { get; set; }
        public string CommandName { get; set; }
        public int ExecutionsCount { get; set; }

        public CommandStats()
        {

        }

        public CommandStats(string commandName, int executionsCount)
        {
            CommandName = commandName;
            ExecutionsCount = executionsCount;
        }
    }
}
