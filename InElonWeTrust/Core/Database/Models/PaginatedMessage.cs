using InElonWeTrust.Core.Services.Pagination;

namespace InElonWeTrust.Core.Database.Models
{
    public class PaginatedMessage
    {
        public int Id { get; set; }
        public string MessageId { get; set; }
        public PaginationContentType ContentType { get; set; }
        public string Parameter { get; set; }
        public int CurrentPage { get; set; }
    }
}
