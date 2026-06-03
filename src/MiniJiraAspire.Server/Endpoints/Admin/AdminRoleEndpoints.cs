using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Endpoints.Admin;

public static class AdminRoleEndpoints
{
    public static void MapAdminRoleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/roles")
            .WithTags("Admin - Roles")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        group.MapPatch("/{userId}", ChangeUserRole)
            .WithName("ChangeUserRole")
            .WithSummary("Change the role of a user (admin)");
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
