using System.Net;
using System.Net.Http.Json;
using MiniJira.IntegrationTests.Infrastructure;
using MiniJiraAspire.Server.Features.Tasks.Commands;
using MiniJiraAspire.Server.Models;

namespace MiniJira.IntegrationTests.Endpoints;

public class TaskEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TaskEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<Guid> SeedProjectAsync()
    {
        // Create a project directly via the API so tasks have a valid ProjectId
        var response = await _client.PostAsJsonAsync("/api/projects", new { Name = "Test Project", Description = "Desc" });
        response.EnsureSuccessStatusCode();
        var project = await response.Content.ReadFromJsonAsync<ProjectDto>();
        return project!.Id;
    }

    private async Task<TaskItemDto> CreateTaskAsync(Guid projectId, string title = "Test Task")
    {
        var command = new CreateTaskCommand(title, "Test Description", projectId.ToString());
        var response = await _client.PostAsJsonAsync("/api/tasks", command);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TaskItemDto>())!;
    }

    [Fact]
    public async Task CreateTask_ReturnsCreatedWithTask()
    {
        var projectId = await SeedProjectAsync();
        var command = new CreateTaskCommand("Integration Test Task", "Description", projectId.ToString());

        var response = await _client.PostAsJsonAsync("/api/tasks", command);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var task = await response.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(task);
        Assert.Equal("Integration Test Task", task!.Title);
        Assert.Equal("Open", task.Status);
        Assert.Equal("Medium", task.Priority);
    }

    [Fact]
    public async Task GetTask_ExistingTask_ReturnsOk()
    {
        var projectId = await SeedProjectAsync();
        var created = await CreateTaskAsync(projectId);

        var response = await _client.GetAsync($"/api/tasks/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var task = await response.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(task);
        Assert.Equal(created.Id, task!.Id);
    }

    [Fact]
    public async Task GetTask_NonExisting_Returns404()
    {
        var response = await _client.GetAsync($"/api/tasks/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAllTasks_ReturnsOkWithList()
    {
        var projectId = await SeedProjectAsync();
        await CreateTaskAsync(projectId, "Task A");
        await CreateTaskAsync(projectId, "Task B");

        var response = await _client.GetAsync("/api/tasks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tasks = await response.Content.ReadFromJsonAsync<TaskItemDto[]>();
        Assert.NotNull(tasks);
        Assert.True(tasks!.Length >= 2);
    }

    [Fact]
    public async Task DeleteTask_ExistingTask_ReturnsNoContent()
    {
        var projectId = await SeedProjectAsync();
        var created = await CreateTaskAsync(projectId);

        var response = await _client.DeleteAsync($"/api/tasks/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify task is gone
        var getResponse = await _client.GetAsync($"/api/tasks/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    private record ProjectDto(Guid Id, string Name, string? Description);
}
