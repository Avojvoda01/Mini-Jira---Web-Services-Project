using Moq;
using MiniJiraAspire.Server.Features.Tasks.Commands;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MinirJira.Test.Features.Tasks;

public class ChangeStatusHandlerTests
{
    private readonly Mock<ITaskRepository> _repoMock = new();
    private readonly ChangeStatusHandler _handler;

    public ChangeStatusHandlerTests()
    {
        _handler = new ChangeStatusHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_CallsChangeStatusAsync()
    {
        var taskId = Guid.NewGuid();
        var command = new ChangeStatusCommand(taskId.ToString(), "InProgress");

        await _handler.Handle(command, CancellationToken.None);

        _repoMock.Verify(r => r.ChangeStatusAsync(taskId, "InProgress", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidTaskId_ThrowsFormatException()
    {
        var command = new ChangeStatusCommand("invalid", "Open");

        await Assert.ThrowsAsync<FormatException>(
            () => _handler.Handle(command, CancellationToken.None));
    }
}
