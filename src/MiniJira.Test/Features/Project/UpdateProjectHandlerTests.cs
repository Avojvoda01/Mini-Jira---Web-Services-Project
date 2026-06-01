using Moq;
using MiniJiraAspire.Server.Features.Project.Commands;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;
namespace MinirJira.Test.Features.Project;
public class UpdateProjectHandlerTests
{
    private readonly Mock<IProjectRepository> _repoMock = new();
    private readonly UpdateProjectHandler _handler;

    public UpdateProjectHandlerTests()
    {
        _handler = new UpdateProjectHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_UpdatesProjectAndReturnsDto()
    {
        var id = Guid.NewGuid();
        var command = new UpdateProjectCommand(id, "Updated", "New Desc");
        var updated = new MiniJiraAspire.Server.Models.Project
        {
            Id = id,
            Name = "Updated",
            Description = "New Desc",
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _repoMock.Setup(r => r.UpdateAsync(id, "Updated", "New Desc", It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(id, result.Id);
        Assert.Equal("Updated", result.Name);
        Assert.Equal("New Desc", result.Description);
    }
}
