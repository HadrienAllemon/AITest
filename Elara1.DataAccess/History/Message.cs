namespace Elara1.DataAccess.History
{
    public class Message
    {
        public Message(string _MessageContent, string _role)
        {
            MessageContent = _MessageContent;
            Role = _role;
            CreatedAt = DateTime.Now;
        }

        public int Id { get; set; }
        public int ConversationId { get; set; }
        public string Role { get; set; }
        public string MessageContent { get; set; } = String.Empty;
        public DateTime CreatedAt { get; set; }
        public Conversation Conversation { get; set; } = null!;
    }
}
