namespace MiniJiraAspire.Server.Models.CommentDTO.Request
{
    public sealed record CreateCommentRequest(string Content, Guid? UserId);

    public sealed record UpdateCommentRequest(string Content);
}
