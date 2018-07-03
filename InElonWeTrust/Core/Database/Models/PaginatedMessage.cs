namespace InElonWeTrust.Core.Database.Models
{
    public class PaginatedMessage
    {
        public int ID { get; set; }
        public ulong MessageID { get; set; }
        public int CurrentPage { get; set; }
    }
}
