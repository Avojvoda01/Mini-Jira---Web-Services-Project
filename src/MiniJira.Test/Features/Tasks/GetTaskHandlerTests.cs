using Moq;
using MiniJiraAspire.Server.Features.Tasks.Queries;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MinirJira.Test.Features.Tasks;

public class GetTaskHandlerTests
{
    private readonly Mock<ITaskRepository> _repoMock = new();
    private readonly GetTaskHandler _handler;

    public GetTaskHandlerTests()
    {
        _handler = new GetTaskHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingTask_ReturnsDto()
    {
        var taskId = Guid.NewGuid();
        var task = new TaskItem
        {
            Id = taskId,
            Title = "Test",
            Status = "Open",
            Priority = "High",
            ProjectId = Guid.NewGuid(),
            CreatedAtUtc = DateTime.UtcNow
        };
        _repoMock.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var result = await _handler.Handle(new GetTaskQuery(taskId.ToString()), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(taskId, result!.Id);
        Assert.Equal("Test", result.Title);
    }

    [Fact]
    public async Task Handle_WithNonExistingTask_ReturnsNull()
    {
        var taskId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        var result = await _handler.Handle(new GetTaskQuery(taskId.ToString()), CancellationToken.None);

        Assert.Null(result);
    }
}
