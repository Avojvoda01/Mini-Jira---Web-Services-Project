using Moq;
using MiniJiraAspire.Server.Features.Epic.Commands;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;
namespace MinirJira.Test.Features.Epic;
public class UpdateEpicHandlerTests
{
    private readonly Mock<IEpicRepository> _repoMock = new();
    private readonly UpdateEpicHandler _handler;

    public UpdateEpicHandlerTests()
    {
        _handler = new UpdateEpicHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_CallsRepositoryUpdate()
    {
        var id = Guid.NewGuid();
        var command = new UpdateEpicCommand(id, "Updated", "New Desc");

        _repoMock.Setup(r => r.UpdateAsync(id, "Updated", "New Desc", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MiniJiraAspire.Server.Models.Epic
            {
                Id = id,
                Name = "Updated",
                Description = "New Desc",
                ProjectId = Guid.NewGuid(),
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            });

        await _handler.Handle(command, CancellationToken.None);

        _repoMock.Verify(r => r.UpdateAsync(id, "Updated", "New Desc", It.IsAny<CancellationToken>()), Times.Once);
    }
}
