using System.ComponentModel.DataAnnotations;

namespace MiniJiraAspire.Server.Models;


public record UserDto(string Id, string Email, string DisplayName, string Role);

public record EpicDto(int Id, string Name, string Description);

public record CreateEpicRequest(
    [property: Required, StringLength(100, MinimumLength = 3)] string Name,
    [property: StringLength(2000)] string? Description);


public record UpdateEpicRequest(
    [property: StringLength(100, MinimumLength = 3)] string Name,
    [property: StringLength(2000)] string? Description);
