using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Endpoints.Comments;

public static class CommentEndpoints
{
    public static void MapCommentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tasks/{taskId}/comments")
            .WithTags("Comments");

        group.MapPost("/", CreateComment)
            .Produces<CommentDto>(StatusCodes.Status201Created)
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

    private static async Task<Created<CommentDto>> CreateComment(
        string taskId,
        CreateCommentRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var comment = await mediator.Send(new CreateCommentCommand(taskId, request.Content, request.UserId), ct);
        return TypedResults.Created($"/api/tasks/{taskId}/comments/{comment.Id}", comment);
    }

    private static async Task<Ok<CommentDto[]>> GetComments(
        string taskId,
        IMediator mediator,
        CancellationToken ct)
    {
        var comments = await mediator.Send(new GetCommentsForTaskQuery(taskId), ct);
        return TypedResults.Ok(comments);
    }

    private static async Task<Results<Ok<CommentDto>, ProblemHttpResult>> UpdateComment(
        string taskId,
        Guid commentId,
        UpdateCommentRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var comment = await mediator.Send(new UpdateCommentCommand(taskId, commentId, request.Content), ct);
        return comment is null
            ? TypedResults.Problem($"Comment with id {commentId} not found", statusCode: StatusCodes.Status404NotFound)
            : TypedResults.Ok(comment);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> DeleteComment(
        string taskId,
        Guid commentId,
        IMediator mediator,
        CancellationToken ct)
    {
        var deleted = await mediator.Send(new DeleteCommentCommand(taskId, commentId), ct);
        return deleted
            ? TypedResults.NoContent()
            : TypedResults.Problem($"Comment with id {commentId} not found", statusCode: StatusCodes.Status404NotFound);
    }
}
