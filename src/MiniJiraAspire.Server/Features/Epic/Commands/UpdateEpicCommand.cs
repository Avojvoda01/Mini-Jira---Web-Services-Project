using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Epic.Commands;

public class UpdateEpicHandler(IEpicRepository repository) : IRequestHandler<UpdateEpicCommand>
{
    public Task Handle(UpdateEpicCommand request, CancellationToken ct)
        => repository.UpdateAsync(request.Id, request.Name, request.Description, ct);
}
