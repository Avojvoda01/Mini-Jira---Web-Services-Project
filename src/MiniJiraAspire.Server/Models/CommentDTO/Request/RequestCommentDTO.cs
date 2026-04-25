namespace MiniJiraAspire.Server.Models.CommentDTO.Request
{
    public sealed record CreateCommentRequest(string Content, int? UserId);

    public sealed record UpdateCommentRequest(string Content);
}
