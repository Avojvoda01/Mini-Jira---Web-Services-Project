using Moq;
using MiniJiraAspire.Server.Features.Comment.Commands;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;
using CommentEntity = MiniJiraAspire.Server.Models.Comment;
using TaskEntity = MiniJiraAspire.Server.Models.TaskItem;
namespace MinirJira.Test.Features.Comment;
public class CreateCommentHandlerTests
{
    private readonly Mock<ICommentRepository> _repoMock = new();
    private readonly Mock<ITaskRepository> _taskRepoMock = new();
    private readonly CreateCommentHandler _handler;

    public CreateCommentHandlerTests()
    {
        _handler = new CreateCommentHandler(_repoMock.Object, _taskRepoMock.Object);
    }

    private void SetupTaskExists(Guid taskId) =>
        _taskRepoMock.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TaskEntity { Id = taskId, Title = "Task", ProjectId = Guid.NewGuid() });

    [Fact]
    public async Task Handle_CreatesCommentAndReturnsDto()
    {
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        SetupTaskExists(taskId);
        var command = new CreateCommentCommand(taskId.ToString(), "Test content", userId);
        var created = new CommentEntity
        {
            Id = Guid.NewGuid(),
            TaskId = taskId.ToString(),
            UserId = userId,
            Content = "Test content",
            CreatedAtUtc = DateTime.UtcNow
        };

        _repoMock.Setup(r => r.CreateAsync(It.IsAny<CommentEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(created.Id, result!.Id);
        Assert.Equal("Test content", result.Content);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(taskId.ToString(), result.TaskId);
        _repoMock.Verify(r => r.CreateAsync(It.Is<CommentEntity>(c =>
            c.Content == "Test content" && c.UserId == userId && c.TaskId == taskId.ToString()
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTaskDoesNotExist_ReturnsNull()
    {
        var taskId = Guid.NewGuid();
        var command = new CreateCommentCommand(taskId.ToString(), "Content", Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Null(result);
        _repoMock.Verify(r => r.CreateAsync(It.IsAny<CommentEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidTaskId_ReturnsNull()
    {
        var command = new CreateCommentCommand("not-a-guid", "Content", Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Null(result);
        _repoMock.Verify(r => r.CreateAsync(It.IsAny<CommentEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
