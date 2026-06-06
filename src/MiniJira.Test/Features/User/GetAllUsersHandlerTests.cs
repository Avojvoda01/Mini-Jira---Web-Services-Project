using Moq;
using MiniJiraAspire.Server.Features.User.Queries;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;
using UserEntity = MiniJiraAspire.Server.Models.User;
namespace MinirJira.Test.Features.User;
public class GetAllUsersHandlerTests
{
    private readonly Mock<IUserRepository> _repoMock = new();
    private readonly GetAllUsersHandler _handler;

    public GetAllUsersHandlerTests()
    {
        _handler = new GetAllUsersHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsAllUsers()
    {
        var users = new List<UserEntity>
        {
            new() { Id = Guid.NewGuid(), Email = "a@test.com", DisplayName = "User A", Role = UserRole.ProjectMember, PasswordHash = "hash" },
            new() { Id = Guid.NewGuid(), Email = "b@test.com", DisplayName = "User B", Role = UserRole.Admin, PasswordHash = "hash" }
        };
        _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(users);

        var result = await _handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("a@test.com", result[0].Email);
        Assert.Equal("b@test.com", result[1].Email);
    }

    [Fact]
    public async Task Handle_EmptyList_ReturnsEmpty()
    {
        _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<UserEntity>());

        var result = await _handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

        Assert.Empty(result);
    }
}
