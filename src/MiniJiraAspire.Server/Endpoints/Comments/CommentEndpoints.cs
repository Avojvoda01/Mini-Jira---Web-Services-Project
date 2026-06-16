using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Endpoints.Comments;

public static class CommentEndpoints
{
    public static void MapCommentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/tasks/{taskId:guid}/comments")
            .WithTags("Comments")
            .RequireAuthorization();

        group.MapPost("/", CreateComment)
            .Produces<CommentDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("CreateComment")
            .WithSummary("Create a comment on a task");

        group.MapGet("/", GetComments)
            .Produces<CommentDto[]>(StatusCodes.Status200OK)
            .WithName("GetCommentsForTask")
            .WithSummary("Get all comments for a task");

        group.MapPut("/{commentId:guid}", UpdateComment)
            .Produces<CommentDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("UpdateComment")
            .WithSummary("Edit an existing comment");

        group.MapDelete("/{commentId:guid}", DeleteComment)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("DeleteComment")
            .WithSummary("Delete a comment");
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(sub, out var id) ? id : null;
    }

    private static async Task<Results<Created<CommentDto>, ProblemHttpResult>> CreateComment(
        string taskId,
        CreateCommentRequest request,
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken ct)
    {
        var comment = await mediator.Send(new CreateCommentCommand(taskId, request.Content, GetUserId(user)), ct);
        return comment is null
            ? TypedResults.Problem($"Task with id {taskId} not found", statusCode: StatusCodes.Status404NotFound)
            : TypedResults.Created($"/api/v1/tasks/{taskId}/comments/{comment.Id}", comment);
    }

    private static async Task<Ok<CommentDto[]>> GetComments(
        string taskId,
        IMediator mediator,
        CancellationToken ct)
    {
        var comments = await mediator.Send(new GetCommentsForTaskQuery(taskId), ct);
        return TypedResults.Ok(comments);
    }

    private static async Task<IResult> UpdateComment(
        string taskId,
        Guid commentId,
        UpdateCommentRequest request,
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken ct)
    {
        var existing = await mediator.Send(new GetCommentByIdQuery(taskId, commentId), ct);
        if (existing is null)
            return Results.Problem($"Comment with id {commentId} not found", statusCode: StatusCodes.Status404NotFound);

        if (!CanModify(user, existing.UserId))
            return Results.Problem("You are not allowed to edit this comment.", statusCode: StatusCodes.Status403Forbidden);

        var updated = await mediator.Send(new UpdateCommentCommand(taskId, commentId, request.Content), ct);
        return updated is null
            ? Results.Problem($"Comment with id {commentId} not found", statusCode: StatusCodes.Status404NotFound)
            : Results.Ok(updated);
    }

    private static async Task<IResult> DeleteComment(
        string taskId,
        Guid commentId,
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken ct)
    {
        var existing = await mediator.Send(new GetCommentByIdQuery(taskId, commentId), ct);
        if (existing is null)
            return Results.Problem($"Comment with id {commentId} not found", statusCode: StatusCodes.Status404NotFound);

        if (!CanModify(user, existing.UserId))
            return Results.Problem("You are not allowed to delete this comment.", statusCode: StatusCodes.Status403Forbidden);

        var deleted = await mediator.Send(new DeleteCommentCommand(taskId, commentId), ct);
        return deleted
            ? Results.NoContent()
            : Results.Problem($"Comment with id {commentId} not found", statusCode: StatusCodes.Status404NotFound);
    }

    private static bool CanModify(ClaimsPrincipal user, Guid? commentUserId)
        => user.IsInRole("Admin") || (GetUserId(user) is { } id && id == commentUserId);
}
