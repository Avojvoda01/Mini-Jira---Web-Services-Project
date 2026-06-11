using System.Net;
using System.Net.Http.Json;
using MiniJira.IntegrationTests.Infrastructure;
using MiniJiraAspire.Server.Models;
namespace MiniJira.IntegrationTests.Endpoints;
public class CommentEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CommentEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<Guid> SeedProjectAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/projects", new { Name = "Comment Test Project", Description = "Desc" });
        response.EnsureSuccessStatusCode();
        var project = await response.Content.ReadFromJsonAsync<ProjectDto>();
        return project!.Id;
    }

    private async Task<TaskItemDto> CreateTaskAsync(Guid projectId)
    {
        var command = new CreateTaskCommand("Comment Test Task", "Description", projectId.ToString());
        var response = await _client.PostAsJsonAsync("/api/v1/tasks", command);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TaskItemDto>())!;
    }

    private async Task<(string TaskId, CommentDto Comment)> CreateCommentAsync(string? taskId = null, string content = "Test comment")
    {
        if (taskId is null)
        {
            var projectId = await SeedProjectAsync();
            var task = await CreateTaskAsync(projectId);
            taskId = task.Id.ToString();
        }

        var response = await _client.PostAsJsonAsync($"/api/v1/tasks/{taskId}/comments", new { Content = content, UserId = (Guid?)null });
        response.EnsureSuccessStatusCode();
        var comment = await response.Content.ReadFromJsonAsync<CommentDto>();
        return (taskId, comment!);
    }

    [Fact]
    public async Task CreateComment_ReturnsCreatedWithComment()
    {
        var projectId = await SeedProjectAsync();
        var task = await CreateTaskAsync(projectId);

        var response = await _client.PostAsJsonAsync($"/api/v1/tasks/{task.Id}/comments", new { Content = "A new comment", UserId = (Guid?)null });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var comment = await response.Content.ReadFromJsonAsync<CommentDto>();
        Assert.NotNull(comment);
        Assert.Equal("A new comment", comment!.Content);
        Assert.Equal(task.Id.ToString(), comment.TaskId);
    }

    [Fact]
    public async Task CreateComment_NonExistingTask_Returns404()
    {
        var fakeTaskId = Guid.NewGuid();

        var response = await _client.PostAsJsonAsync($"/api/v1/tasks/{fakeTaskId}/comments", new { Content = "Comment", UserId = (Guid?)null });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetComments_ReturnsOkWithList()
    {
        var (taskId, _) = await CreateCommentAsync();
        await CreateCommentAsync(taskId, "Second comment");

        var response = await _client.GetAsync($"/api/v1/tasks/{taskId}/comments");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var comments = await response.Content.ReadFromJsonAsync<CommentDto[]>();
        Assert.NotNull(comments);
        Assert.True(comments!.Length >= 2);
    }

    [Fact]
    public async Task UpdateComment_ExistingComment_ReturnsOkWithUpdated()
    {
        var (taskId, created) = await CreateCommentAsync();

        var response = await _client.PutAsJsonAsync($"/api/v1/tasks/{taskId}/comments/{created.Id}", new { Content = "Updated comment" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<CommentDto>();
        Assert.NotNull(updated);
        Assert.Equal("Updated comment", updated!.Content);
    }

    [Fact]
    public async Task UpdateComment_NonExistingComment_Returns404()
    {
        var projectId = await SeedProjectAsync();
        var task = await CreateTaskAsync(projectId);

        var response = await _client.PutAsJsonAsync($"/api/v1/tasks/{task.Id}/comments/{Guid.NewGuid()}", new { Content = "Updated" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteComment_ExistingComment_ReturnsNoContent()
    {
        var (taskId, created) = await CreateCommentAsync();

        var response = await _client.DeleteAsync($"/api/v1/tasks/{taskId}/comments/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteComment_NonExistingComment_Returns404()
    {
        var projectId = await SeedProjectAsync();
        var task = await CreateTaskAsync(projectId);

        var response = await _client.DeleteAsync($"/api/v1/tasks/{task.Id}/comments/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
