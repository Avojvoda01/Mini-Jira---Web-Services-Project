using Microsoft.AspNetCore.Http.HttpResults;
using MiniJiraAspire.Server.Models.CommentDTO.Request;
using MiniJiraAspire.Server.Persistence.Repositories.Interfaces;

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
        ICommentRepository repository,
        CancellationToken cancellationToken)
    {
        var comment = await repository.CreateAsync(taskId, request, cancellationToken);
        return TypedResults.Created($"/api/tasks/{taskId}/comments/{comment.Id}", comment);
    }

    private static async Task<NoContent> UpdateComment(
        string taskId,
        int commentId,
        UpdateCommentRequest request,
        ICommentRepository repository,
        CancellationToken cancellationToken)
    {
        await repository.UpdateAsync(taskId, commentId, request, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> DeleteComment(
        string taskId,
        int commentId,
        ICommentRepository repository,
        CancellationToken cancellationToken)
    {
        await repository.DeleteAsync(taskId, commentId, cancellationToken);
        return TypedResults.NoContent();
    }
}
