namespace MiniJiraAspire.Server.Models
{
    public class Comment
    {
        public Guid Id { get; set; }
        public string TaskId { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
