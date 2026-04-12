namespace Microsoft.Extensions.Hosting.Auth.Login;

public static class LoginEndpoint
{
    public static void MapLogin(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/login", async (
                LoginCommand command,
                CancellationToken ct) =>
            {
                // TODO: implement logic
                return Results.Ok(new { Token = "fake-jwt" });
            })
            .WithName("Login")
            .WithTags("Auth")
            .WithSummary("Login user");
    }
}

public record LoginCommand(string Email, string Password);