using Moq;
using MiniJiraAspire.Server.Features.User.Commands;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;
namespace MinirJira.Test.Features.User;
public class DeleteUserHandlerTests
{
    private readonly Mock<IUserRepository> _repoMock = new();
    private readonly DeleteUserHandler _handler;

    public DeleteUserHandlerTests()
    {
        _handler = new DeleteUserHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingUser_ReturnsTrue()
    {
        var userId = Guid.NewGuid().ToString();
        _repoMock.Setup(r => r.DeleteAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _handler.Handle(new DeleteUserCommand(userId), CancellationToken.None);

        Assert.True(result);
    }

    [Fact]
    public async Task Handle_NonExistingUser_ReturnsFalse()
    {
        _repoMock.Setup(r => r.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _handler.Handle(new DeleteUserCommand(Guid.NewGuid().ToString()), CancellationToken.None);

        Assert.False(result);
    }
}
