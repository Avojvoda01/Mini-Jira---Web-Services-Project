using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using MiniJiraAspire.Server.Features.Comment;
using MiniJiraAspire.Server.Models.CommentDTO.Request;

namespace Microsoft.Extensions.Hosting.Comments;

public static class CommentEndpoints
{
    public static void MapCommentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tasks/{taskId}/comments")
            .WithTags("Comments");

        group.MapPost("/", CreateComment)
            .Produces<CommentDTO>(StatusCodes.Status201Created)
            .WithName("CreateComment")
            .WithSummary("Create a comment on a task");

        group.MapPut("/{commentId:int}", UpdateComment)
            .Produces(StatusCodes.Status204NoContent)
            .WithName("UpdateComment")
            .WithSummary("Edit an existing comment");

        group.MapDelete("/{commentId:int}", DeleteComment)
            .Produces(StatusCodes.Status204NoContent)
            .WithName("DeleteComment")
            .WithSummary("Delete a comment");
    }

    private static async Task<Created<CommentDTO>> CreateComment(
        string taskId,
        CreateCommentRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var comment = await mediator.Send(new CreateCommentCommand(taskId, request.Content, request.UserId), ct);
        return TypedResults.Created($"/api/tasks/{taskId}/comments/{comment.Id}", comment);
    }

    private static async Task<NoContent> UpdateComment(
        string taskId,
        int commentId,
        UpdateCommentRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        await mediator.Send(new UpdateCommentCommand(taskId, commentId, request.Content), ct);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> DeleteComment(
        string taskId,
        int commentId,
        IMediator mediator,
        CancellationToken ct)
    {
        await mediator.Send(new DeleteCommentCommand(taskId, commentId), ct);
        return TypedResults.NoContent();
    }
}
