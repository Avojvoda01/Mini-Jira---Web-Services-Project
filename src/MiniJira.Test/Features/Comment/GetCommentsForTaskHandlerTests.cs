using Moq;
using MiniJiraAspire.Server.Features.Comment.Queries;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;
using CommentEntity = MiniJiraAspire.Server.Models.Comment;
namespace MinirJira.Test.Features.Comment;
public class GetCommentsForTaskHandlerTests
{
    private readonly Mock<ICommentRepository> _repoMock = new();
    private readonly GetCommentsForTaskHandler _handler;

    public GetCommentsForTaskHandlerTests()
    {
        _handler = new GetCommentsForTaskHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsCommentDtos()
    {
        var taskId = Guid.NewGuid().ToString();
        var comments = new List<CommentEntity>
        {
            new() { Id = Guid.NewGuid(), TaskId = taskId, Content = "First", CreatedAtUtc = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), TaskId = taskId, Content = "Second", CreatedAtUtc = DateTime.UtcNow }
        };

        _repoMock.Setup(r => r.GetAllAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comments);

        var result = await _handler.Handle(new GetCommentsForTaskQuery(taskId), CancellationToken.None);

        Assert.Equal(2, result.Length);
        Assert.Equal("First", result[0].Content);
        Assert.Equal("Second", result[1].Content);
    }

    [Fact]
    public async Task Handle_NoComments_ReturnsEmptyArray()
    {
        var taskId = Guid.NewGuid().ToString();

        _repoMock.Setup(r => r.GetAllAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CommentEntity>());

        var result = await _handler.Handle(new GetCommentsForTaskQuery(taskId), CancellationToken.None);

        Assert.Empty(result);
    }
}
