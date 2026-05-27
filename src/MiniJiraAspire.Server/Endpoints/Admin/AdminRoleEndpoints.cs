using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using MiniJiraAspire.Server.Features.User.Commands;
using MiniJiraAspire.Server.Models;

namespace Microsoft.Extensions.Hosting.Admin.Roles;

public static class AdminRoleEndpoints
{
    public static void MapAdminRoleEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/admin/roles/{userId}", ChangeUserRole)
            .WithName("ChangeUserRole")
            .WithTags("Admin - Roles")
            .WithSummary("Change the role of a user (admin)")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));
    }

    private static async Task<Results<Ok<UserDto>, ValidationProblem, ProblemHttpResult>> ChangeUserRole(
        string userId,
        ChangeUserRoleRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new ChangeUserRoleCommand(userId, request.Role), ct);

        if (result.ValidationErrors is not null)
        {
            return TypedResults.ValidationProblem(result.ValidationErrors);
        }

        if (result.NotFound)
        {
            return TypedResults.Problem($"User with id {userId} not found", statusCode: StatusCodes.Status404NotFound);
        }

        return TypedResults.Ok(result.User!);
    }
}

public record ChangeUserRoleRequest(string Role);