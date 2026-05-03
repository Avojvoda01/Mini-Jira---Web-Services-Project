namespace MiniJiraAspire.Server.Models.CommentDTO.Request
{
    public sealed record CommentDTO(
        Guid Id,
        string TaskId,
        Guid? UserId,
        string Content,
        DateTime CreatedAtUtc,
        DateTime? UpdatedAtUtc);
}
