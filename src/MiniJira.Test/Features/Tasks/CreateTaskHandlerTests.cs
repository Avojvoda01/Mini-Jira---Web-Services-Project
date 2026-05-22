using Moq;
using MiniJiraAspire.Server.Features.Tasks.Commands;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MinirJira.Test.Features.Tasks;

public class CreateTaskHandlerTests
{
    private readonly Mock<ITaskRepository> _repoMock = new();
    private readonly CreateTaskHandler _handler;

    public CreateTaskHandlerTests()
    {
        _handler = new CreateTaskHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_CreatesTaskAndReturnsDto()
    {
        var projectId = Guid.NewGuid();
        var command = new CreateTaskCommand("Test Task", "Description", projectId.ToString());
        var createdTask = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Test Task",
            Description = "Description",
            ProjectId = projectId,
            Status = "Open",
            Priority = "Medium",
            CreatedAtUtc = DateTime.UtcNow
        };

        _repoMock.Setup(r => r.CreateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(createdTask.Id, result.Id);
        Assert.Equal("Test Task", result.Title);
        Assert.Equal("Description", result.Description);
        Assert.Equal("Open", result.Status);
        Assert.Equal(projectId, result.ProjectId);
        _repoMock.Verify(r => r.CreateAsync(It.Is<TaskItem>(t =>
            t.Title == "Test Task" && t.Description == "Description" && t.ProjectId == projectId
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidProjectId_ThrowsFormatException()
    {
        var command = new CreateTaskCommand("Title", null, "not-a-guid");

        await Assert.ThrowsAsync<FormatException>(
            () => _handler.Handle(command, CancellationToken.None));
    }
}
