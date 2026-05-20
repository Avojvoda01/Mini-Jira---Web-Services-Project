using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Services.Auth;

public interface IJwtTokenService
{
    string CreateToken(User user);
}
