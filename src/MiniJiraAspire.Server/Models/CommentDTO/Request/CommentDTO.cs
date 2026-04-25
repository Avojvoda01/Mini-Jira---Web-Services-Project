namespace MiniJiraAspire.Server.Models.CommentDTO.Request
{
    public sealed record CommentDTO(
        int Id,
        string TaskId,
        int? UserId,
        string Content,
        DateTime CreatedAtUtc,
        DateTime? UpdatedAtUtc);
}
