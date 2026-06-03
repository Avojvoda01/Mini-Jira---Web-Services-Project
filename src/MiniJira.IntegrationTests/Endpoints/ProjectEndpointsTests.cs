using System.Net;
using System.Net.Http.Json;
using MiniJira.IntegrationTests.Infrastructure;
using MiniJiraAspire.Server.Models;
namespace MiniJira.IntegrationTests.Endpoints;
public class ProjectEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProjectEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<ProjectDto> CreateProjectAsync(string name = "Test Project", string description = "Desc")
    {
        var response = await _client.PostAsJsonAsync("/api/v1/projects", new { Name = name, Description = description });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ProjectDto>())!;
    }

    [Fact]
    public async Task CreateProject_ReturnsCreatedWithProject()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/projects", new { Name = "New Project", Description = "A description" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var project = await response.Content.ReadFromJsonAsync<ProjectDto>();
        Assert.NotNull(project);
        Assert.Equal("New Project", project!.Name);
        Assert.Equal("A description", project.Description);
    }

    [Fact]
    public async Task GetProject_ExistingProject_ReturnsOk()
    {
        var created = await CreateProjectAsync();

        var response = await _client.GetAsync($"/api/v1/projects/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var project = await response.Content.ReadFromJsonAsync<ProjectDto>();
        Assert.NotNull(project);
        Assert.Equal(created.Id, project!.Id);
    }

    [Fact]
    public async Task GetProject_NonExisting_Returns404()
    {
        var response = await _client.GetAsync($"/api/v1/projects/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAllProjects_ReturnsOkWithList()
    {
        await CreateProjectAsync("Project A");
        await CreateProjectAsync("Project B");

        var response = await _client.GetAsync("/api/v1/projects");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var projects = await response.Content.ReadFromJsonAsync<ProjectDto[]>();
        Assert.NotNull(projects);
        Assert.True(projects!.Length >= 2);
    }

    [Fact]
    public async Task UpdateProject_ExistingProject_ReturnsOkWithUpdatedProject()
    {
        var created = await CreateProjectAsync();

        var response = await _client.PutAsJsonAsync($"/api/v1/projects/{created.Id}", new { Name = "Updated", Description = "Updated Desc" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<ProjectDto>();
        Assert.NotNull(updated);
        Assert.Equal("Updated", updated!.Name);
        Assert.Equal("Updated Desc", updated.Description);
    }

    [Fact]
    public async Task UpdateProject_NonExisting_Returns404()
    {
        var response = await _client.PutAsJsonAsync($"/api/projects/{Guid.NewGuid()}", new { Name = "Updated", Description = "Updated Desc" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProject_ExistingProject_ReturnsNoContent()
    {
        var created = await CreateProjectAsync();

        var response = await _client.DeleteAsync($"/api/v1/projects/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/api/v1/projects/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
