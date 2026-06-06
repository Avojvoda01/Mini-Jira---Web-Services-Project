using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using MiniJira.IntegrationTests.Infrastructure;
using MiniJiraAspire.Server.Models;
namespace MiniJira.IntegrationTests.Endpoints;
public class UserEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public UserEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    private async Task<UserDto> CreateUserAsync(string email = "user@test.com", string password = "Password123!", string displayName = "TestUser")
    {
        var response = await _client.PostAsJsonAsync("/api/v1/users", new { Email = email, Password = password, DisplayName = displayName });
        response.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<UserDto>(await response.Content.ReadAsStringAsync(), _jsonOptions)!;
    }

    [Fact]
    public async Task CreateUser_ValidRequest_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/users", new { Email = "create@test.com", Password = "Password123!", DisplayName = "CreateUser" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var user = JsonSerializer.Deserialize<UserDto>(await response.Content.ReadAsStringAsync(), _jsonOptions);
        Assert.NotNull(user);
        Assert.Equal("create@test.com", user!.Email);
        Assert.Equal("CreateUser", user.DisplayName);
    }

    [Fact]
    public async Task CreateUser_DuplicateEmail_Returns409()
    {
        var email = $"dup-{Guid.NewGuid()}@test.com";
        await CreateUserAsync(email: email, displayName: $"User-{Guid.NewGuid()}");

        var response = await _client.PostAsJsonAsync("/api/v1/users", new { Email = email, Password = "Password123!", DisplayName = $"Other-{Guid.NewGuid()}" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_InvalidEmail_Returns422()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/users", new { Email = "not-an-email", Password = "Password123!", DisplayName = $"User-{Guid.NewGuid()}" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_ShortPassword_Returns422()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/users", new { Email = $"short-{Guid.NewGuid()}@test.com", Password = "short", DisplayName = $"User-{Guid.NewGuid()}" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_ReturnsOkWithList()
    {
        await CreateUserAsync(email: $"list-{Guid.NewGuid()}@test.com", displayName: $"List-{Guid.NewGuid()}");

        var response = await _client.GetAsync("/api/v1/users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var users = JsonSerializer.Deserialize<List<UserDto>>(await response.Content.ReadAsStringAsync(), _jsonOptions);
        Assert.NotNull(users);
        Assert.True(users!.Count >= 1);
    }

    [Fact]
    public async Task GetUser_ExistingUser_ReturnsOk()
    {
        var created = await CreateUserAsync(email: $"get-{Guid.NewGuid()}@test.com", displayName: $"Get-{Guid.NewGuid()}");

        var response = await _client.GetAsync($"/api/v1/users/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = JsonSerializer.Deserialize<UserDto>(await response.Content.ReadAsStringAsync(), _jsonOptions);
        Assert.NotNull(user);
        Assert.Equal(created.Id, user!.Id);
    }

    [Fact]
    public async Task GetUser_NonExisting_Returns404()
    {
        var response = await _client.GetAsync($"/api/v1/users/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_ExistingUser_ReturnsNoContent()
    {
        var created = await CreateUserAsync(email: $"del-{Guid.NewGuid()}@test.com", displayName: $"Del-{Guid.NewGuid()}");

        var response = await _client.DeleteAsync($"/api/v1/users/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/api/v1/users/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_NonExisting_Returns404()
    {
        var response = await _client.DeleteAsync($"/api/v1/users/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ChangeUserRole_ValidRole_ReturnsOk()
    {
        var created = await CreateUserAsync(email: $"role-{Guid.NewGuid()}@test.com", displayName: $"Role-{Guid.NewGuid()}");

        var response = await _client.PatchAsJsonAsync($"/api/v1/users/{created.Id}/role", new { Role = "Admin" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = JsonSerializer.Deserialize<UserDto>(await response.Content.ReadAsStringAsync(), _jsonOptions);
        Assert.NotNull(user);
        Assert.Equal(UserRole.Admin, user!.Role);
    }

    [Fact]
    public async Task ChangeUserRole_InvalidRole_Returns422()
    {
        var created = await CreateUserAsync(email: $"badrole-{Guid.NewGuid()}@test.com", displayName: $"BadRole-{Guid.NewGuid()}");

        var response = await _client.PatchAsJsonAsync($"/api/v1/users/{created.Id}/role", new { Role = "InvalidRole" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task ChangeUserRole_NonExistingUser_Returns404()
    {
        var response = await _client.PatchAsJsonAsync($"/api/v1/users/{Guid.NewGuid()}/role", new { Role = "Admin" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
