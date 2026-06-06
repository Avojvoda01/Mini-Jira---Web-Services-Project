using Moq;
using Microsoft.AspNetCore.Identity;
using MiniJiraAspire.Server.Features.User.Commands;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;
using UserEntity = MiniJiraAspire.Server.Models.User;
namespace MinirJira.Test.Features.User;
public class CreateUserHandlerTests
{
    private readonly Mock<IUserRepository> _repoMock = new();
    private readonly Mock<IPasswordHasher<UserEntity>> _hasherMock = new();
    private readonly CreateUserHandler _handler;

    public CreateUserHandlerTests()
    {
        _hasherMock.Setup(h => h.HashPassword(It.IsAny<UserEntity>(), It.IsAny<string>())).Returns("hashed");
        _handler = new CreateUserHandler(_repoMock.Object, _hasherMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsSuccess()
    {
        var userId = Guid.NewGuid();
        _repoMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repoMock.Setup(r => r.DisplayNameExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity u, CancellationToken _) => { u.Id = userId; return u; });

        var result = await _handler.Handle(new CreateUserCommand("test@test.com", "Password123!", "TestUser"), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.User);
        Assert.Equal("test@test.com", result.User!.Email);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsEmailConflict()
    {
        _repoMock.Setup(r => r.EmailExistsAsync("dup@test.com", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _repoMock.Setup(r => r.DisplayNameExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _handler.Handle(new CreateUserCommand("dup@test.com", "Password123!", "UniqueUser"), CancellationToken.None);

        Assert.True(result.EmailConflict);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Handle_DuplicateDisplayName_ReturnsValidationError()
    {
        _repoMock.Setup(r => r.DisplayNameExistsAsync("Taken", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _handler.Handle(new CreateUserCommand("new@test.com", "Password123!", "Taken"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("DisplayName", result.Errors.Keys);
    }

    [Fact]
    public async Task Handle_InvalidEmail_ReturnsValidationError()
    {
        var result = await _handler.Handle(new CreateUserCommand("not-an-email", "Password123!", "User"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.True(result.Errors.Count > 0);
    }

    [Fact]
    public async Task Handle_ShortPassword_ReturnsValidationError()
    {
        var result = await _handler.Handle(new CreateUserCommand("ok@test.com", "short", "User"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.True(result.Errors.Count > 0);
    }
}
