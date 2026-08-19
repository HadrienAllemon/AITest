namespace Elara1.DataAccess.History
{
    public class Role
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public ICollection<Message> Messages { get; } = [];
    }
}
