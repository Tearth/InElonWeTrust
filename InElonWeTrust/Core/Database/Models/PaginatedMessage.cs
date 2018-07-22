using InElonWeTrust.Core.Services.Cache;

namespace InElonWeTrust.Core.Database.Models
{
    public class PaginatedMessage
    {
        public int Id { get; set; }
        public string GuildId { get; set; }
        public string MessageId { get; set; }
        public CacheContentType ContentType { get; set; }
        public string Parameter { get; set; }
        public int CurrentPage { get; set; }
    }
}
