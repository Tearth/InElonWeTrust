using InElonWeTrust.Core.Services.Pagination;

namespace InElonWeTrust.Core.Database.Models
{
    public class PaginatedMessage
    {
        public int ID { get; set; }
        public string MessageID { get; set; }
        public PaginationContentType ContentType { get; set; } 
        public int CurrentPage { get; set; }
    }
}
