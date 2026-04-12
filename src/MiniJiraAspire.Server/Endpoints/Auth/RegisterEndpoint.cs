namespace Microsoft.Extensions.Hosting.Auth.Register;

public static class RegisterEndpoint
{
    public static void MapRegister(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/register", async (
                RegisterCommand command,
                CancellationToken ct) =>
            {
                // TODO: implement logic
                return Results.Created("/api/auth/register", new { Message = "User registered" });
            })
            .WithName("Register")
            .WithTags("Auth")
            .WithSummary("Register a new user");
    }
}

public record RegisterCommand(string Email, string Password, string DisplayName);
