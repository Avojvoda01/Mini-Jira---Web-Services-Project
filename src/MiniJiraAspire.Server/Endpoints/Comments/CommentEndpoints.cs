namespace Microsoft.Extensions.Hosting.Comments;

public static class CommentEndpoints
{
    public static void MapCommentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/tasks/{taskId}/comments", async (
                string taskId,
                CreateCommentCommand command,
                CancellationToken ct) =>
            {
                // TODO: implement logic
                return Results.Created($"/api/tasks/{taskId}/comments/new-id", new { Id = "new-id" });
            })
            .WithName("CreateComment")
            .WithTags("Comments")
            .WithSummary("Create a comment on a task");

        app.MapPut("/api/tasks/{taskId}/comments/{commentId}", async (
                string taskId,
                string commentId,
                UpdateCommentCommand command,
                CancellationToken ct) =>
            {
                // TODO: implement logic
                return Results.Ok();
            })
            .WithName("UpdateComment")
            .WithTags("Comments")
            .WithSummary("Edit an existing comment");

        app.MapDelete("/api/tasks/{taskId}/comments/{commentId}", async (
                string taskId,
                string commentId,
                CancellationToken ct) =>
            {
                // TODO: implement logic
                return Results.NoContent();
            })
            .WithName("DeleteComment")
            .WithTags("Comments")
            .WithSummary("Delete a comment");
    }
}

public record CreateCommentCommand(string Content);
public record UpdateCommentCommand(string Content);