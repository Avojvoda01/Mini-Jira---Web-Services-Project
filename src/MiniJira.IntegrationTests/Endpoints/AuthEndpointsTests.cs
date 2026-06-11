using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using MiniJira.IntegrationTests.Infrastructure;
using MiniJiraAspire.Server.Models;
namespace MiniJira.IntegrationTests.Endpoints;
public class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public AuthEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<UserDto> RegisterUserAsync(string email = "test@example.com", string password = "Password1", string displayName = "TestUser")
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new { Email = email, Password = password, DisplayName = displayName });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<UserDto>(JsonOptions))!;
    }

    [Fact]
    public async Task Register_ValidRequest_ReturnsCreatedWithUser()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register",
            new { Email = "register@test.com", Password = "Password1", DisplayName = "RegisterUser" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
        Assert.NotNull(user);
        Assert.Equal("register@test.com", user!.Email);
        Assert.Equal("RegisterUser", user.DisplayName);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        await RegisterUserAsync("dup@test.com", "Password1", "First");

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register",
            new { Email = "dup@test.com", Password = "Password1", DisplayName = "Second" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_InvalidEmail_Returns422()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register",
            new { Email = "not-an-email", Password = "Password1", DisplayName = "User" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Register_ShortPassword_Returns422()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register",
            new { Email = "short@test.com", Password = "Ab1", DisplayName = "User" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        await RegisterUserAsync("login@test.com", "Password1", "LoginUser");

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { Email = "login@test.com", Password = "Password1" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions);
        Assert.NotNull(loginResponse);
        Assert.False(string.IsNullOrEmpty(loginResponse!.Token));
        Assert.Equal("login@test.com", loginResponse.User.Email);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        await RegisterUserAsync("wrongpw@test.com", "Password1", "WrongPwUser");

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { Email = "wrongpw@test.com", Password = "WrongPassword1" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_NonExistingUser_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { Email = "noone@test.com", Password = "Password1" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
