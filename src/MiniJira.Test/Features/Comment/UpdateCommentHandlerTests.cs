using Moq;
using MiniJiraAspire.Server.Features.Comment.Commands;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;
using CommentEntity = MiniJiraAspire.Server.Models.Comment;
namespace MinirJira.Test.Features.Comment;
public class UpdateCommentHandlerTests
{
    private readonly Mock<ICommentRepository> _repoMock = new();
    private readonly UpdateCommentHandler _handler;

    public UpdateCommentHandlerTests()
    {
        _handler = new UpdateCommentHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingComment_ReturnsUpdatedDto()
    {
        var taskId = Guid.NewGuid().ToString();
        var commentId = Guid.NewGuid();
        var updated = new CommentEntity
        {
            Id = commentId,
            TaskId = taskId,
            UserId = Guid.NewGuid(),
            Content = "Updated content",
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _repoMock.Setup(r => r.UpdateAsync(taskId, commentId, "Updated content", It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        var result = await _handler.Handle(new UpdateCommentCommand(taskId, commentId, "Updated content"), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(commentId, result!.Id);
        Assert.Equal("Updated content", result.Content);
    }

    [Fact]
    public async Task Handle_NonExistingComment_ReturnsNull()
    {
        var taskId = Guid.NewGuid().ToString();
        var commentId = Guid.NewGuid();

        _repoMock.Setup(r => r.UpdateAsync(taskId, commentId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CommentEntity?)null);

        var result = await _handler.Handle(new UpdateCommentCommand(taskId, commentId, "Content"), CancellationToken.None);

        Assert.Null(result);
    }
}
