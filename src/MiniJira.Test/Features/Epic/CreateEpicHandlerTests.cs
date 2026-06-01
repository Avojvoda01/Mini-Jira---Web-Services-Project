using Moq;
using MiniJiraAspire.Server.Features.Epic.Commands;
using MiniJiraAspire.Server.Persistence.Repositories;
using EpicEntity = MiniJiraAspire.Server.Models.Epic;
namespace MinirJira.Test.Features.Epic;
public class CreateEpicHandlerTests
{
    private readonly Mock<IEpicRepository> _repoMock = new();
    private readonly CreateEpicHandler _handler;

    public CreateEpicHandlerTests()
    {
        _handler = new CreateEpicHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_CreatesEpicAndReturnsDto()
    {
        var projectId = Guid.NewGuid();
        var command = new CreateEpicCommand("Test Epic", "A description", projectId);
        var created = new EpicEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test Epic",
            Description = "A description",
            ProjectId = projectId,
            CreatedAtUtc = DateTime.UtcNow
        };

        _repoMock.Setup(r => r.CreateAsync(It.IsAny<EpicEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(created.Id, result.Id);
        Assert.Equal("Test Epic", result.Name);
        Assert.Equal("A description", result.Description);
        Assert.Equal(projectId, result.ProjectId);
        _repoMock.Verify(r => r.CreateAsync(It.Is<EpicEntity>(e =>
            e.Name == "Test Epic" && e.Description == "A description" && e.ProjectId == projectId
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNullDescription_CreatesEpic()
    {
        var projectId = Guid.NewGuid();
        var command = new CreateEpicCommand("No Desc", null, projectId);
        var created = new EpicEntity
        {
            Id = Guid.NewGuid(),
            Name = "No Desc",
            Description = null,
            ProjectId = projectId,
            CreatedAtUtc = DateTime.UtcNow
        };

        _repoMock.Setup(r => r.CreateAsync(It.IsAny<EpicEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal("No Desc", result.Name);
        Assert.Equal(string.Empty, result.Description);
    }
}
