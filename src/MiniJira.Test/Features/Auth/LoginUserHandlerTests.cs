using Microsoft.AspNetCore.Identity;
using Moq;
using MiniJiraAspire.Server.Features.Auth.Commands;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;
using MiniJiraAspire.Server.Services.Auth;
using UserEntity = MiniJiraAspire.Server.Models.User;
namespace MinirJira.Test.Features.Auth;
public class LoginUserHandlerTests
{
    private readonly Mock<IUserRepository> _repoMock = new();
    private readonly Mock<IPasswordHasher<UserEntity>> _hasherMock = new();
    private readonly Mock<IJwtTokenService> _jwtMock = new();
    private readonly LoginUserHandler _handler;

    public LoginUserHandlerTests()
    {
        _handler = new LoginUserHandler(_repoMock.Object, _hasherMock.Object, _jwtMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsLoginResponse()
    {
        var user = new UserEntity { Id = Guid.NewGuid(), Email = "test@test.com", PasswordHash = "hashed", DisplayName = "Test" };
        _repoMock.Setup(r => r.GetByEmailAsync("test@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _hasherMock.Setup(h => h.VerifyHashedPassword(user, "hashed", "password123"))
            .Returns(PasswordVerificationResult.Success);
        _jwtMock.Setup(j => j.CreateToken(user)).Returns("jwt-token");

        var result = await _handler.Handle(new LoginUserCommand("test@test.com", "password123"), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("jwt-token", result!.Token);
        Assert.Equal(user.Id.ToString(), result.User.Id);
        Assert.Equal("test@test.com", result.User.Email);
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsNull()
    {
        _repoMock.Setup(r => r.GetByEmailAsync("unknown@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        var result = await _handler.Handle(new LoginUserCommand("unknown@test.com", "password"), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_WrongPassword_ReturnsNull()
    {
        var user = new UserEntity { Id = Guid.NewGuid(), Email = "test@test.com", PasswordHash = "hashed", DisplayName = "Test" };
        _repoMock.Setup(r => r.GetByEmailAsync("test@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _hasherMock.Setup(h => h.VerifyHashedPassword(user, "hashed", "wrong"))
            .Returns(PasswordVerificationResult.Failed);

        var result = await _handler.Handle(new LoginUserCommand("test@test.com", "wrong"), CancellationToken.None);

        Assert.Null(result);
    }
}
