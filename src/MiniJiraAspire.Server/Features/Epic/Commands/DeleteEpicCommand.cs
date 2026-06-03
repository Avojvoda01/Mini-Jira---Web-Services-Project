using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Epic.Commands;

public class DeleteEpicHandler(IEpicRepository repository) : IRequestHandler<DeleteEpicCommand, bool>
{
    public Task<bool> Handle(DeleteEpicCommand request, CancellationToken ct)
        => repository.DeleteAsync(request.Id, ct);
}
