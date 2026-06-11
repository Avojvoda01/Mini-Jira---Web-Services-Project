using Moq;
using MiniJiraAspire.Server.Features.Comment.Commands;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;
namespace MinirJira.Test.Features.Comment;
public class DeleteCommentHandlerTests
{
    private readonly Mock<ICommentRepository> _repoMock = new();
    private readonly DeleteCommentHandler _handler;

    public DeleteCommentHandlerTests()
    {
        _handler = new DeleteCommentHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingComment_ReturnsTrue()
    {
        var taskId = Guid.NewGuid().ToString();
        var commentId = Guid.NewGuid();

        _repoMock.Setup(r => r.DeleteAsync(taskId, commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(new DeleteCommentCommand(taskId, commentId), CancellationToken.None);

        Assert.True(result);
    }

    [Fact]
    public async Task Handle_NonExistingComment_ReturnsFalse()
    {
        var taskId = Guid.NewGuid().ToString();
        var commentId = Guid.NewGuid();

        _repoMock.Setup(r => r.DeleteAsync(taskId, commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(new DeleteCommentCommand(taskId, commentId), CancellationToken.None);

        Assert.False(result);
    }
}
