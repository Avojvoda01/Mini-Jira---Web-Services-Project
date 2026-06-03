using MediatR;
using Microsoft.AspNetCore.Identity;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;
using MiniJiraAspire.Server.Services.Auth;
using UserEntity = MiniJiraAspire.Server.Models.User;

namespace MiniJiraAspire.Server.Features.Auth.Commands;

public class LoginUserHandler(
    IUserRepository repository,
    IPasswordHasher<UserEntity> passwordHasher,
    IJwtTokenService jwtTokenService) : IRequestHandler<LoginUserCommand, LoginResponse?>
{
    public async Task<LoginResponse?> Handle(LoginUserCommand request, CancellationToken ct)
    {
        var user = await repository.GetByEmailAsync(request.Email, ct);

        if (user is null)
        {
            return null;
        }

        var passwordResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (passwordResult == PasswordVerificationResult.Failed)
        {
            return null;
        }

        var token = jwtTokenService.CreateToken(user);
        var userDto = new UserDto(user.Id.ToString(), user.Email, user.DisplayName, user.Role);

        return new LoginResponse(token, userDto);
    }
}
