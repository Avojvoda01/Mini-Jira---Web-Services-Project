using Moq;
using MiniJiraAspire.Server.Features.Project.Queries;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;
namespace MinirJira.Test.Features.Project;
public class GetProjectByIdHandlerTests
{
    private readonly Mock<IProjectRepository> _repoMock = new();
    private readonly GetProjectByIdHandler _handler;

    public GetProjectByIdHandlerTests()
    {
        _handler = new GetProjectByIdHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingProject_ReturnsDto()
    {
        var id = Guid.NewGuid();
        var project = new MiniJiraAspire.Server.Models.Project
        {
            Id = id,
            Name = "My Project",
            Description = "Desc",
            CreatedAtUtc = DateTime.UtcNow,
            Members = []
        };

        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var result = await _handler.Handle(new GetProjectByIdQuery(id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
        Assert.Equal("My Project", result.Name);
    }

    [Fact]
    public async Task Handle_NonExistingProject_ReturnsNull()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MiniJiraAspire.Server.Models.Project?)null);

        var result = await _handler.Handle(new GetProjectByIdQuery(id), CancellationToken.None);

        Assert.Null(result);
    }
}
