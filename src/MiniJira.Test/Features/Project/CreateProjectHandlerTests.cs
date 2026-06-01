using Moq;
using MiniJiraAspire.Server.Features.Project.Commands;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;
namespace MinirJira.Test.Features.Project;
public class CreateProjectHandlerTests
{
    private readonly Mock<IProjectRepository> _repoMock = new();
    private readonly CreateProjectHandler _handler;

    public CreateProjectHandlerTests()
    {
        _handler = new CreateProjectHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_CreatesProjectAndReturnsDto()
    {
        var command = new CreateProjectCommand("Test Project", "A description");
        var created = new MiniJiraAspire.Server.Models.Project
        {
            Id = Guid.NewGuid(),
            Name = "Test Project",
            Description = "A description",
            CreatedAtUtc = DateTime.UtcNow
        };

        _repoMock.Setup(r => r.CreateAsync(It.IsAny<MiniJiraAspire.Server.Models.Project>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(created.Id, result.Id);
        Assert.Equal("Test Project", result.Name);
        Assert.Equal("A description", result.Description);
        _repoMock.Verify(r => r.CreateAsync(It.Is<MiniJiraAspire.Server.Models.Project>(p =>
            p.Name == "Test Project" && p.Description == "A description"
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNullDescription_CreatesProject()
    {
        var command = new CreateProjectCommand("No Desc", null);
        var created = new MiniJiraAspire.Server.Models.Project
        {
            Id = Guid.NewGuid(),
            Name = "No Desc",
            Description = null!,
            CreatedAtUtc = DateTime.UtcNow
        };

        _repoMock.Setup(r => r.CreateAsync(It.IsAny<MiniJiraAspire.Server.Models.Project>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal("No Desc", result.Name);
        Assert.Null(result.Description);
    }
}
