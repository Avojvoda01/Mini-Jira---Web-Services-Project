using System.ComponentModel.DataAnnotations;

namespace MiniJiraAspire.Server.Models;

public record EpicDto(Guid Id, string Name, string Description);

public record CreateEpicRequest(
    [property: Required, StringLength(100, MinimumLength = 3)] string Name,
    [property: StringLength(2000)] string? Description);


public record UpdateEpicRequest(
    [property: StringLength(100, MinimumLength = 3)] string Name,
    [property: StringLength(2000)] string? Description);

public record ProjectDto(Guid Id, string Name, string Description);

public record CreateProjectRequest(
    [property: Required, StringLength(100, MinimumLength = 3)] string Name,
    [property: StringLength(2000)] string Description);

public record UpdateProjectRequest(
    [property: StringLength(100, MinimumLength = 3)] string Name,
    [property: StringLength(2000)] string Description);