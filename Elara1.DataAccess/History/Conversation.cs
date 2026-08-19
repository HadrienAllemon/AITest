namespace Elara1.DataAccess.History
{
    public class Conversation
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedAt {get;set;}

        public ICollection<Message> Messages { get; } = [];
    }
}
