using Moq;
using MiniJiraAspire.Server.Features.Tasks.Commands;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MinirJira.Test.Features.Tasks;

public class AssignEpicHandlerTests
{
    private readonly Mock<ITaskRepository> _repoMock = new();
    private readonly AssignEpicHandler _handler;

    public AssignEpicHandlerTests()
    {
        _handler = new AssignEpicHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidEpicId_CallsAssignEpicAsync()
    {
        var taskId = Guid.NewGuid();
        var epicId = Guid.NewGuid();
        var command = new AssignEpicCommand(taskId.ToString(), epicId.ToString());

        await _handler.Handle(command, CancellationToken.None);

        _repoMock.Verify(r => r.AssignEpicAsync(taskId, epicId, It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNullEpicId_PassesNullToRepository()
    {
        var taskId = Guid.NewGuid();
        var command = new AssignEpicCommand(taskId.ToString(), null);

        await _handler.Handle(command, CancellationToken.None);

        _repoMock.Verify(r => r.AssignEpicAsync(taskId, null, It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyEpicId_PassesNullToRepository()
    {
        var taskId = Guid.NewGuid();
        var command = new AssignEpicCommand(taskId.ToString(), "");

        await _handler.Handle(command, CancellationToken.None);

        _repoMock.Verify(r => r.AssignEpicAsync(taskId, null, It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidTaskId_ThrowsFormatException()
    {
        var command = new AssignEpicCommand("not-a-guid", null);

        await Assert.ThrowsAsync<FormatException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithInvalidEpicId_ThrowsFormatException()
    {
        var taskId = Guid.NewGuid();
        var command = new AssignEpicCommand(taskId.ToString(), "not-a-guid");

        await Assert.ThrowsAsync<FormatException>(
            () => _handler.Handle(command, CancellationToken.None));
    }
}
