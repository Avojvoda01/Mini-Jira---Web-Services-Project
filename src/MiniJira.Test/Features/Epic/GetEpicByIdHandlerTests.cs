using Moq;
using MiniJiraAspire.Server.Features.Epic.Queries;
using MiniJiraAspire.Server.Persistence.Repositories;
using EpicEntity = MiniJiraAspire.Server.Models.Epic;
namespace MinirJira.Test.Features.Epic;
public class GetEpicByIdHandlerTests
{
    private readonly Mock<IEpicRepository> _repoMock = new();
    private readonly GetEpicByIdHandler _handler;

    public GetEpicByIdHandlerTests()
    {
        _handler = new GetEpicByIdHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingEpic_ReturnsDto()
    {
        var id = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var epic = new EpicEntity
        {
            Id = id,
            Name = "My Epic",
            Description = "Desc",
            ProjectId = projectId,
            CreatedAtUtc = DateTime.UtcNow
        };

        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(epic);

        var result = await _handler.Handle(new GetEpicByIdQuery(id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
        Assert.Equal("My Epic", result.Name);
        Assert.Equal(projectId, result.ProjectId);
    }

    [Fact]
    public async Task Handle_NonExistingEpic_ReturnsNull()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EpicEntity?)null);

        var result = await _handler.Handle(new GetEpicByIdQuery(id), CancellationToken.None);

        Assert.Null(result);
    }
}
