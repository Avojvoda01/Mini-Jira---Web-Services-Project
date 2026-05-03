using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Epic;

public record UpdateEpicCommand(int Id, string Name, string? Description) : IRequest;

public class UpdateEpicHandler(IEpicRepository repository) : IRequestHandler<UpdateEpicCommand>
{
    public Task Handle(UpdateEpicCommand request, CancellationToken ct)
        => repository.UpdateAsync(request.Id, new UpdateEpicRequest(request.Name, request.Description), ct);
}
