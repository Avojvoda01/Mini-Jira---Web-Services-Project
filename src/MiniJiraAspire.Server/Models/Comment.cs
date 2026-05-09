namespace MiniJiraAspire.Server.Models
{
    public class Comment : BaseEntity
    {
        public string TaskId { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
