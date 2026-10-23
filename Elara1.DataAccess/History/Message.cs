namespace Elara1.DataAccess.History
{
    public class Message
{
    public Message() { }   // for EF

    public Message(string messageContent, int roleId)
    {
        MessageContent = messageContent;
        RoleId = roleId;
        CreatedAt = DateTime.Now;
    }

    public int Id { get; set; }
    public int ConversationId { get; set; }
    public int RoleId { get; set; }
    public string MessageContent { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public Conversation Conversation { get; set; } = null!;
    public Role Role { get; set; } = null!;
}

}
