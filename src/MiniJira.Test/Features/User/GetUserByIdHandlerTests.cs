using Moq;
using MiniJiraAspire.Server.Features.User.Queries;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;
using UserEntity = MiniJiraAspire.Server.Models.User;
namespace MinirJira.Test.Features.User;
public class GetUserByIdHandlerTests
{
    private readonly Mock<IUserRepository> _repoMock = new();
    private readonly GetUserByIdHandler _handler;

    public GetUserByIdHandlerTests()
    {
        _handler = new GetUserByIdHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingUser_ReturnsUserDto()
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity { Id = userId, Email = "test@test.com", DisplayName = "Test", Role = UserRole.User, PasswordHash = "hash" };
        _repoMock.Setup(r => r.GetByIdAsync(userId.ToString(), It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _handler.Handle(new GetUserByIdQuery(userId.ToString()), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("test@test.com", result!.Email);
    }

    [Fact]
    public async Task Handle_NonExistingUser_ReturnsNull()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((UserEntity?)null);

        var result = await _handler.Handle(new GetUserByIdQuery(Guid.NewGuid().ToString()), CancellationToken.None);

        Assert.Null(result);
    }
}
