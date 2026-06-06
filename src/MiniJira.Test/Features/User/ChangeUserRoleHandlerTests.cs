using Moq;
using MiniJiraAspire.Server.Features.User.Commands;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;
using UserEntity = MiniJiraAspire.Server.Models.User;
namespace MinirJira.Test.Features.User;
public class ChangeUserRoleHandlerTests
{
    private readonly Mock<IUserRepository> _repoMock = new();
    private readonly ChangeUserRoleHandler _handler;

    public ChangeUserRoleHandlerTests()
    {
        _handler = new ChangeUserRoleHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRole_ReturnsSuccess()
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity { Id = userId, Email = "test@test.com", DisplayName = "Test", Role = UserRole.Admin, PasswordHash = "hash" };
        _repoMock.Setup(r => r.ChangeRoleAsync(userId.ToString(), UserRole.Admin, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _handler.Handle(new ChangeUserRoleCommand(userId.ToString(), "Admin"), CancellationToken.None);

        Assert.NotNull(result.User);
        Assert.False(result.NotFound);
        Assert.Equal(UserRole.Admin, result.User!.Role);
    }

    [Fact]
    public async Task Handle_InvalidRole_ReturnsValidationError()
    {
        var result = await _handler.Handle(new ChangeUserRoleCommand(Guid.NewGuid().ToString(), "InvalidRole"), CancellationToken.None);

        Assert.NotNull(result.ValidationErrors);
        Assert.Contains("Role", result.ValidationErrors!.Keys);
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsNotFound()
    {
        _repoMock.Setup(r => r.ChangeRoleAsync(It.IsAny<string>(), It.IsAny<UserRole>(), It.IsAny<CancellationToken>())).ReturnsAsync((UserEntity?)null);

        var result = await _handler.Handle(new ChangeUserRoleCommand(Guid.NewGuid().ToString(), "Admin"), CancellationToken.None);

        Assert.True(result.NotFound);
        Assert.Null(result.User);
    }
}
