namespace InElonWeTrust.Core.Database.Models
{
    public class MessageToSubscribe
    {
        public int Id { get; set; }
        public string MessageId { get; set; }

        public MessageToSubscribe()
        {

        }

        public MessageToSubscribe(string messageId)
        {
            MessageId = messageId;
        }
    }
}
