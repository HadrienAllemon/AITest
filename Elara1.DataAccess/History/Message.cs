namespace Elara1.DataAccess.History
{
    public class Message
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public int RoleId { get; set; }
        public string MessageContent { get; set; } = String.Empty;
        public DateTime CreatedAt {get;set;}
        public string Test {get;set;}

        public Conversation Conversation { get; set; } = null!;
        public Role Role { get; set; } = null!;
    }
}
