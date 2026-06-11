using Microsoft.AspNetCore.Identity;
using Moq;
using MiniJiraAspire.Server.Features.Auth.Commands;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;
using UserEntity = MiniJiraAspire.Server.Models.User;
namespace MinirJira.Test.Features.Auth;
public class RegisterUserHandlerTests
{
    private readonly Mock<IUserRepository> _repoMock = new();
    private readonly Mock<IPasswordHasher<UserEntity>> _hasherMock = new();
    private readonly RegisterUserHandler _handler;

    public RegisterUserHandlerTests()
    {
        _handler = new RegisterUserHandler(_repoMock.Object, _hasherMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsSuccess()
    {
        var createdUser = new UserEntity { Id = Guid.NewGuid(), Email = "new@test.com", PasswordHash = "hashed", DisplayName = "NewUser" };
        _repoMock.Setup(r => r.EmailExistsAsync("new@test.com", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repoMock.Setup(r => r.DisplayNameExistsAsync("NewUser", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>())).ReturnsAsync(createdUser);
        _hasherMock.Setup(h => h.HashPassword(It.IsAny<UserEntity>(), "Password1")).Returns("hashed");

        var result = await _handler.Handle(new RegisterUserCommand("new@test.com", "Password1", "NewUser"), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.User);
        Assert.Equal("new@test.com", result.User!.Email);
        Assert.Equal("NewUser", result.User.DisplayName);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsEmailConflict()
    {
        _repoMock.Setup(r => r.EmailExistsAsync("dup@test.com", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _repoMock.Setup(r => r.DisplayNameExistsAsync("User", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _handler.Handle(new RegisterUserCommand("dup@test.com", "Password1", "User"), CancellationToken.None);

        Assert.True(result.EmailConflict);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Handle_DuplicateDisplayName_ReturnsValidationError()
    {
        _repoMock.Setup(r => r.DisplayNameExistsAsync("Taken", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _handler.Handle(new RegisterUserCommand("new@test.com", "Password1", "Taken"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.False(result.EmailConflict);
        Assert.True(result.Errors.ContainsKey("DisplayName"));
    }

    [Fact]
    public async Task Handle_InvalidEmail_ReturnsValidationError()
    {
        var result = await _handler.Handle(new RegisterUserCommand("not-an-email", "Password1", "User"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.True(result.Errors.ContainsKey("Email"));
    }

    [Fact]
    public async Task Handle_ShortPassword_ReturnsValidationError()
    {
        var result = await _handler.Handle(new RegisterUserCommand("test@test.com", "Ab1", "User"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.True(result.Errors.ContainsKey("Password"));
    }
}
