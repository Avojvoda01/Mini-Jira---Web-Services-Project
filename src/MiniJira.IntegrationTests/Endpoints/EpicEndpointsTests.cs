using System.Net;
using System.Net.Http.Json;
using MiniJira.IntegrationTests.Infrastructure;
using MiniJiraAspire.Server.Models;
namespace MiniJira.IntegrationTests.Endpoints;
public class EpicEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EpicEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<Guid> CreateProjectAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/projects", new { Name = "Epic Test Project", Description = "Desc" });
        response.EnsureSuccessStatusCode();
        var project = await response.Content.ReadFromJsonAsync<ProjectDto>();
        return project!.Id;
    }

    private async Task<EpicDto> CreateEpicAsync(string name = "Test Epic", string? description = "Desc", Guid? projectId = null)
    {
        var pid = projectId ?? await CreateProjectAsync();
        var response = await _client.PostAsJsonAsync("/api/epics", new { Name = name, Description = description, ProjectId = pid });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<EpicDto>())!;
    }

    [Fact]
    public async Task CreateEpic_ReturnsCreatedWithEpic()
    {
        var projectId = await CreateProjectAsync();

        var response = await _client.PostAsJsonAsync("/api/epics", new { Name = "New Epic", Description = "A description", ProjectId = projectId });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var epic = await response.Content.ReadFromJsonAsync<EpicDto>();
        Assert.NotNull(epic);
        Assert.Equal("New Epic", epic!.Name);
        Assert.Equal("A description", epic.Description);
        Assert.Equal(projectId, epic.ProjectId);
    }

    [Fact]
    public async Task GetEpic_ExistingEpic_ReturnsOk()
    {
        var created = await CreateEpicAsync();

        var response = await _client.GetAsync($"/api/epics/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var epic = await response.Content.ReadFromJsonAsync<EpicDto>();
        Assert.NotNull(epic);
        Assert.Equal(created.Id, epic!.Id);
    }

    [Fact]
    public async Task GetEpic_NonExisting_Returns404()
    {
        var response = await _client.GetAsync($"/api/epics/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAllEpics_ReturnsOkWithList()
    {
        var projectId = await CreateProjectAsync();
        await CreateEpicAsync("Epic A", projectId: projectId);
        await CreateEpicAsync("Epic B", projectId: projectId);

        var response = await _client.GetAsync("/api/epics");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var epics = await response.Content.ReadFromJsonAsync<EpicDto[]>();
        Assert.NotNull(epics);
        Assert.True(epics!.Length >= 2);
    }

    [Fact]
    public async Task UpdateEpic_ExistingEpic_ReturnsNoContent()
    {
        var created = await CreateEpicAsync();

        var response = await _client.PutAsJsonAsync($"/api/epics/{created.Id}", new { Name = "Updated", Description = "Updated Desc" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/api/epics/{created.Id}");
        var updated = await getResponse.Content.ReadFromJsonAsync<EpicDto>();
        Assert.Equal("Updated", updated!.Name);
        Assert.Equal("Updated Desc", updated.Description);
    }

    [Fact]
    public async Task DeleteEpic_ExistingEpic_ReturnsNoContent()
    {
        var created = await CreateEpicAsync();

        var response = await _client.DeleteAsync($"/api/epics/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/api/epics/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
