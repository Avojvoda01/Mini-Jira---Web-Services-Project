using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Epic;

public record CreateEpicCommand(string Name, string? Description, Guid ProjectId) : IRequest<EpicDto>;

public class CreateEpicHandler(IEpicRepository repository) : IRequestHandler<CreateEpicCommand, EpicDto>
{
    public Task<EpicDto> Handle(CreateEpicCommand request, CancellationToken ct)
        => repository.CreateAsync(new CreateEpicRequest(request.Name, request.Description, request.ProjectId), ct);
}
